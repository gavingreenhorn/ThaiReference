package org.jonecorp.thaireference.config;

import org.jonecorp.thaireference.config.properties.AccountMailProperties;
import org.jonecorp.thaireference.config.properties.AccountProperties;
import org.jonecorp.thaireference.config.properties.AppProperties;
import org.jonecorp.thaireference.config.properties.ReferenceDataProperties;
import org.springframework.boot.context.properties.EnableConfigurationProperties;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.CacheControl;
import org.springframework.lang.NonNull;
import org.springframework.web.servlet.config.annotation.CorsRegistry;
import org.springframework.web.servlet.config.annotation.InterceptorRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;
import org.springframework.web.servlet.mvc.WebContentInterceptor;

@Configuration
@EnableConfigurationProperties({
    AppProperties.class,
    ReferenceDataProperties.class,
    AccountProperties.class,
    AccountMailProperties.class
})
public class WebConfig implements WebMvcConfigurer {

    private final AppProperties appProperties;
    private final ReferenceDataProperties referenceDataProperties;

    public WebConfig(AppProperties appProperties, ReferenceDataProperties referenceDataProperties) {
        this.appProperties = appProperties;
        this.referenceDataProperties = referenceDataProperties;
    }

    @Override
    public void addCorsMappings(@NonNull CorsRegistry registry) {
        String[] origins = appProperties.corsOriginsArray();
        if (origins.length == 0) {
            return;
        }
        registry.addMapping("/**")
                .allowedOrigins(origins)
                .allowedMethods("*")
                .allowedHeaders("*")
                .allowCredentials(true);
    }

    @Override
    public void addInterceptors(@NonNull InterceptorRegistry registry) {
        WebContentInterceptor cacheHeaders = new WebContentInterceptor();
        cacheHeaders.addCacheMapping(
                CacheControl.maxAge(referenceDataProperties.httpCacheMaxAge()).cachePublic(), "/thai/**");
        registry.addInterceptor(cacheHeaders);
    }
}
