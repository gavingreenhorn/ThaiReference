package com.thaireference.snapadmin;

import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.dao.DataAccessException;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import tech.ailef.snapadmin.external.SnapAdmin;
import tech.ailef.snapadmin.external.SnapAdminProperties;
import tech.ailef.snapadmin.external.controller.SnapAdminController;
import tech.ailef.snapadmin.external.dbmapping.DbObjectSchema;
import tech.ailef.snapadmin.external.dbmapping.SnapAdminRepository;
import tech.ailef.snapadmin.external.dto.CompareOperator;
import tech.ailef.snapadmin.external.dto.FacetedSearchRequest;
import tech.ailef.snapadmin.external.dto.PaginatedResult;
import tech.ailef.snapadmin.external.dto.QueryFilter;
import tech.ailef.snapadmin.external.exceptions.InvalidPageException;
import tech.ailef.snapadmin.external.exceptions.SnapAdminException;
import tech.ailef.snapadmin.external.misc.Utils;

/**
 * Replaces stock {@link SnapAdminController#list}: pagination params must not force {@code search()}.
 * Stock bean is removed in {@link SnapAdminControllerBeanFix}.
 */
@Controller
@RequestMapping(value = {"/${snapadmin.baseUrl}", "/${snapadmin.baseUrl}/"})
@ConditionalOnProperty(name = "snapadmin.enabled", havingValue = "true")
public class FixedSnapAdminController extends SnapAdminController {

    @Autowired
    private SnapAdminRepository repository;

    @Autowired
    private SnapAdminProperties properties;

    @Autowired
    private SnapAdmin snapAdmin;

    @Override
    @GetMapping("/model/{className:.+}")
    public String list(
            Model model,
            @PathVariable String className,
            @RequestParam(required = false) Integer page,
            @RequestParam(required = false) String query,
            @RequestParam(required = false) Integer pageSize,
            @RequestParam(required = false) String sortKey,
            @RequestParam(required = false) String sortOrder,
            @RequestParam MultiValueMap<String, String> otherParams,
            HttpServletRequest request,
            HttpServletResponse response) {

        if (page == null) {
            page = 1;
        }
        if (pageSize == null) {
            pageSize = 50;
        }

        DbObjectSchema schema = snapAdmin.findSchemaByClassName(className);
        model.addAttribute("className", schema.getClassName());

        @SuppressWarnings("unchecked")
        Set<QueryFilter> queryFilters = Utils.computeFilters(schema, otherParams);
        if (otherParams.containsKey("remove_field")) {
            List<String> fields = otherParams.get("remove_field");

            for (int i = 0; i < fields.size(); i++) {
                QueryFilter toRemove = new QueryFilter(
                        schema.getFieldByJavaName(fields.get(i)),
                        CompareOperator.valueOf(otherParams.get("remove_op").get(i).toUpperCase()),
                        otherParams.get("remove_value").get(i));
                queryFilters.removeIf(f -> f.equals(toRemove));
            }

            FacetedSearchRequest filterRequest = new FacetedSearchRequest(queryFilters);
            MultiValueMap<String, String> parameterMap = filterRequest.computeParams();

            MultiValueMap<String, String> filteredParams = new LinkedMultiValueMap<>();
            request.getParameterMap().entrySet().stream()
                    .filter(e -> !e.getKey().startsWith("remove_") && !e.getKey().startsWith("filter_"))
                    .forEach(e -> {
                        filteredParams.putIfAbsent(e.getKey(), new ArrayList<>());
                        for (String v : e.getValue()) {
                            if (filteredParams.get(e.getKey()).isEmpty()) {
                                filteredParams.get(e.getKey()).add(v);
                            } else {
                                filteredParams.get(e.getKey()).set(0, v);
                            }
                        }
                    });

            filteredParams.putAll(parameterMap);
            String queryString = Utils.getQueryString(filteredParams);
            String redirectUrl = request.getServletPath() + queryString;
            return "redirect:" + redirectUrl.trim();
        }

        try {
            boolean hasTextQuery = query != null && !query.isBlank();
            boolean hasFilters = queryFilters != null && !queryFilters.isEmpty();
            PaginatedResult<?> result;
            if (hasTextQuery || hasFilters) {
                result = repository.search(
                        schema, hasTextQuery ? query : null, page, pageSize, sortKey, sortOrder, queryFilters);
            } else {
                result = repository.findAll(schema, page, pageSize, sortKey, sortOrder);
            }

            model.addAttribute("title", "Entities | " + schema.getJavaClass().getSimpleName() + " | Index");
            model.addAttribute("page", result);
            model.addAttribute("schema", schema);
            model.addAttribute("activePage", "entities");
            model.addAttribute("sortKey", sortKey);
            model.addAttribute("query", query);
            model.addAttribute("sortOrder", sortOrder);
            model.addAttribute("activeFilters", queryFilters);
            return "snapadmin/model/list";

        } catch (InvalidPageException e) {
            return "redirect:/" + properties.getBaseUrl() + "/model/" + className;
        } catch (SnapAdminException e) {
            model.addAttribute("error", e.getMessage());
            model.addAttribute("errorTitle", "Invalid request");
            model.addAttribute("schema", schema);
            model.addAttribute("activePage", "entities");
            model.addAttribute("sortKey", sortKey);
            model.addAttribute("query", query);
            model.addAttribute("sortOrder", sortOrder);
            model.addAttribute("activeFilters", queryFilters);
            return "snapadmin/model/list";
        } catch (DataAccessException e) {
            model.addAttribute("error", e.getMostSpecificCause().getMessage());
            model.addAttribute("errorTitle", "Search failed");
            model.addAttribute("schema", schema);
            model.addAttribute("activePage", "entities");
            model.addAttribute("sortKey", sortKey);
            model.addAttribute("query", query);
            model.addAttribute("sortOrder", sortOrder);
            model.addAttribute("activeFilters", queryFilters);
            return "snapadmin/model/list";
        }
    }

    @Override
    @GetMapping("/console/run/{queryId}")
    public String consoleRun(
            Model model,
            @RequestParam(required = false) String query,
            @RequestParam(required = false) String queryTitle,
            @RequestParam(required = false) Integer page,
            @RequestParam(required = false) Integer pageSize,
            @PathVariable String queryId) {
        try {
            return super.consoleRun(model, query, queryTitle, page, pageSize, queryId);
        } catch (DataAccessException e) {
            model.addAttribute("errorTitle", "SQL error");
            model.addAttribute("error", e.getMostSpecificCause().getMessage());
            return "snapadmin/console";
        }
    }
}
