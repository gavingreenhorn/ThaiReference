package org.jonecorp.thaireference.dto;

import java.util.List;

public record PagedResultDto<T>(
		List<T> items,
		int page,
		int pageSize,
		long totalItems,
		int totalPages,
		boolean hasNext,
		boolean hasPrevious,
		String filter,
		List<String> availableFilters) {
}
