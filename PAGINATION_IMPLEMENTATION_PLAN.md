# Implementation Plan: Server-Side Pagination for LeetCode Problems

## Overview
Implement server-side pagination with filtering and sorting for the LeetCode problems list to improve performance and scalability.

## Objectives
- Add offset-based pagination to LeetCode problems API
- Implement server-side filtering and sorting
- Update frontend to use server-side pagination
- Maintain caching strategy (per-page caching)
- Set default page size to 20 with options for 10, 50, 100

## Phase 1: Backend - DTOs and Models ✅ COMPLETED

### 1.1 Create Pagination DTOs ✅
**File:** `server/Shared/PaginationDtos.cs` (new file)
- ✅ Create `PagedRequest` record (Page, PageSize)
- ✅ Create `PagedResponse<T>` record (Items, TotalCount, Page, PageSize, TotalPages)
- ✅ Add validation for page/pageSize limits
- ✅ Add `Create()` helper method for automatic calculation

### 1.2 Update LeetCode DTOs ✅
**File:** `server/Features/Leetcode/Dtos/ListLeetcodeProblems.cs`
- ✅ Update `ListLeetcodeProblemsRequest` to include:
  - ✅ Pagination parameters (Page, PageSize) - inherited from PagedRequest
  - ✅ Filter parameters (Difficulty, Category, SearchTerm)
  - ✅ Sort parameters (SortBy, SortOrder)
- ✅ Update `ListLeetcodeProblemsResponse` to use `PagedResponse<LeetcodeProblemDto>`
- ✅ Add FluentValidation rules for all parameters

## Phase 2: Backend - Repository Layer ✅ COMPLETED

### 2.1 Add Pagination to Repository Interface ✅
**File:** `server/Common/Interfaces/IRepository.cs`
- ✅ Add `GetPagedAsync<T>()` method signature with pagination, filtering, sorting parameters

### 2.2 Implement Pagination in Repository ✅
**File:** `server/Common/EntityRepository.cs`
- ✅ Implement `GetPagedAsync()` method
- ✅ Use EF Core `.Skip()` and `.Take()` for pagination
- ✅ Calculate total count for pagination metadata
- ✅ Support dynamic sorting and filtering

## Phase 3: Backend - Service Layer

### 3.1 Update LeetCode Service
**File:** `server/Features/Leetcode/LeetcodeService.cs`
- Modify `ListLeetcodeProblems()` to use paginated repository method
- Build filter expressions based on request parameters
- Update cache key to include pagination/filter/sort parameters
- Reduce cache TTL or implement cache invalidation strategy

### 3.2 Update Cache Strategy
**File:** `server/Common/Cache/MemoryRequestCache.cs` (if needed)
- Ensure cache keys are unique per page/filter/sort combination
- Consider cache size limits

## Phase 4: Backend - Controller Layer

### 4.1 Update Controller
**File:** `server/Features/Leetcode/LeetcodeController.cs`
- Update controller action to accept new request parameters
- Ensure proper model binding for query parameters
- Update API documentation/attributes

## Phase 5: Frontend - API Client

### 5.1 Regenerate API Client
- Run API client generation to pick up new types
- Verify generated hooks include pagination parameters

## Phase 6: Frontend - Components

### 6.1 Update LeetCode Table Component
**File:** `client/src/pages/Leetcode/LeetcodeTable/LeetcodeTable.tsx`
- Switch from client-side to server-side pagination in Mantine React Table
- Implement `manualPagination` mode
- Add page size selector (10, 20, 50, 100)
- Handle loading states during pagination
- Implement server-side filtering and sorting
- Update to use new paginated API hook

### 6.2 Update LeetCode Page Component
**File:** `client/src/pages/Leetcode/Leetcode.page.tsx`
- Pass pagination state to table component
- Handle refetch on pagination/filter/sort changes

## Phase 7: Testing & Validation

### 7.1 Backend Testing
- Test pagination with various page sizes
- Test filtering combinations
- Test sorting (ascending/descending)
- Verify cache key uniqueness
- Test edge cases (page out of bounds, invalid parameters)

### 7.2 Frontend Testing
- Test pagination navigation
- Test page size changes
- Test filtering with pagination
- Test sorting with pagination
- Verify loading states
- Test cache behavior

### 7.3 Performance Testing
- Compare response times (before vs after)
- Verify database query efficiency
- Check cache hit rates

## Phase 8: Documentation

### 8.1 Update API Documentation
- Document new pagination parameters
- Add examples for filtering and sorting
- Update Swagger annotations

### 8.2 Code Documentation
- Add XML comments to new methods
- Document pagination strategy in README

## Implementation Order

1. Backend DTOs (Phase 1)
2. Backend Repository (Phase 2)
3. Backend Service (Phase 3)
4. Backend Controller (Phase 4)
5. Regenerate Frontend API Client (Phase 5)
6. Frontend Components (Phase 6)
7. Testing (Phase 7)
8. Documentation (Phase 8)

## Success Criteria

- ✅ Initial page load returns only 20 LeetCode problems (instead of 3000+)
- ✅ Pagination controls work correctly
- ✅ Filtering applies before pagination
- ✅ Sorting works with pagination
- ✅ Backend cache works per-page
- ✅ Performance improvement measurable (< 500ms response time)
- ✅ No breaking changes to existing functionality

## Technical Details

### Pagination Parameters
```
GET /api/v1/leetcode/list-leetcode-problems?page=1&pageSize=20&difficulty=Hard&sortBy=title&sortOrder=asc
```

### Response Format
```json
{
  "responseBody": {
    "items": [...],
    "totalCount": 3000,
    "page": 1,
    "pageSize": 20,
    "totalPages": 150,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Cache Key Strategy
```
leetcode-problems-p{page}-ps{pageSize}-d{difficulty}-c{category}-s{sortBy}-o{sortOrder}-q{searchTerm}
```

## Notes

- This is a learning exercise focusing on LeetCode problems only
- Mock Interviews pagination can follow the same pattern later
- Keep existing client-side pagination as reference until migration complete
- Consider feature flag for gradual rollout if needed

## Files to be Modified/Created

### New Files
- `server/Shared/PaginationDtos.cs`

### Modified Files (Backend)
- `server/Features/Leetcode/Dtos/ListLeetcodeProblems.cs`
- `server/Common/Interfaces/IRepository.cs`
- `server/Common/EntityRepository.cs`
- `server/Features/Leetcode/LeetcodeService.cs`
- `server/Features/Leetcode/LeetcodeController.cs`

### Modified Files (Frontend)
- `client/src/pages/Leetcode/LeetcodeTable/LeetcodeTable.tsx`
- `client/src/pages/Leetcode/Leetcode.page.tsx`

## Estimated Time
- Backend Implementation: 2-3 hours
- Frontend Implementation: 1-2 hours
- Testing & Debugging: 1-2 hours
- **Total: 4-7 hours**
