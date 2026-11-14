package org.jonecorp.thaireference.snapadmin;

import org.springframework.beans.BeansException;
import org.springframework.beans.factory.config.BeanDefinition;
import org.springframework.beans.factory.config.BeanFactoryPostProcessor;
import org.springframework.beans.factory.config.ConfigurableListableBeanFactory;
import org.springframework.beans.factory.support.BeanDefinitionRegistry;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import tech.ailef.snapadmin.external.controller.SnapAdminController;

/** Drops the stock SnapAdmin controller so {@link FixedSnapAdminController} owns the mappings. */
@Configuration
@ConditionalOnProperty(name = "snapadmin.enabled", havingValue = "true")
public class SnapAdminControllerBeanFix {

    @Bean
    static BeanFactoryPostProcessor removeStockSnapAdminController() {
        return new BeanFactoryPostProcessor() {
            @Override
            public void postProcessBeanFactory(ConfigurableListableBeanFactory beanFactory)
                    throws BeansException {
                if (!(beanFactory instanceof BeanDefinitionRegistry registry)) {
                    return;
                }
                for (String name : registry.getBeanDefinitionNames()) {
                    BeanDefinition bd = registry.getBeanDefinition(name);
                    if (SnapAdminController.class.getName().equals(bd.getBeanClassName())) {
                        registry.removeBeanDefinition(name);
                    }
                }
            }
        };
    }
}
