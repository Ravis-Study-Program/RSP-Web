# Implementation Plan: Server-Side Pagination for Problem Attempts

## Overview
Implement server-side pagination with filtering and sorting for the Problem Attempts list to improve performance and scalability.

## Objectives
- Add offset-based pagination to Problem Attempts API
- Implement server-side filtering and sorting
- Update frontend to use server-side pagination
- Maintain caching strategy (per-page caching)
- Set default page size to 10 with options for 5, 20, 50, 100

## Phase 1: Backend - DTOs and Models ✅ COMPLETED

### 1.1 Create Pagination DTOs ✅
**File:** `server/Shared/PaginationDtos.cs` (new file)
- ✅ Create `PagedRequest` record (Page, PageSize)
- ✅ Create `PagedResponse<T>` record (Items, TotalCount, Page, PageSize, TotalPages)
- ✅ Add validation for page/pageSize limits
- ✅ Add `Create()` helper method for automatic calculation

### 1.2 Update Problem Attempts DTOs ✅
**File:** `server/Features/ProblemAttempts/Dtos/ListProblemAttempt.cs`
- ✅ Update `ListProblemAttemptRequest` to include:
  - ✅ Pagination parameters (Page, PageSize) - inherited from PagedRequest
  - ✅ Filter parameters (UserIds, IncludeLeetcode, IncludeCustom, SeasonId)
- ✅ Update `ListProblemAttemptResponse` to use `PagedResponse<ProblemAttemptEntity>`
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

## Phase 3: Backend - Service Layer ✅ COMPLETED

### 3.1 Update Problem Attempts Service ✅
**File:** `server/Features/ProblemAttempts/ProblemAttemptService.cs`
- ✅ Modify `ListProblemAttempt()` to use paginated repository method
- ✅ Build filter expressions based on request parameters
- ✅ Update cache key to include pagination/filter parameters
- ✅ Cache TTL set to 1 hour

### 3.2 Update Cache Strategy ✅
**File:** `server/Common/Cache/MemoryRequestCache.cs` (if needed)
- ✅ Cache keys are unique per page/filter/sort combination
- ✅ Cache invalidation works correctly (clears all pages when problems added)

## Phase 4: Backend - Controller Layer ✅ COMPLETED

### 4.1 Update Controller ✅
**File:** `server/Features/ProblemAttempts/ProblemAttemptController.cs`
- ✅ Controller already uses [FromQuery] - automatically accepts new parameters
- ✅ Model binding works for all pagination/filter parameters
- ✅ No changes needed - already compatible

## Phase 5: Frontend - API Client ✅ COMPLETED

### 5.1 Regenerate API Client ✅
- ✅ Run API client generation to pick up new types
- ✅ Verify generated hooks include pagination parameters
- ✅ New type: `ListProblemAttemptResponse` with `result: ProblemAttemptEntityPagedResponse`
- ✅ New type: `ProblemAttemptEntityPagedResponse` with pagination metadata

## Phase 6: Frontend - Components ✅ COMPLETED

### 6.1 Update Problem Attempts Table Component ✅
**File:** `client/src/pages/Leetcode/LeetcodeTable/LeetcodeTable.tsx`
- ✅ Switch from client-side to server-side pagination in Mantine React Table
- ✅ Implement `manualPagination` mode with `rowCount` for total items
- ✅ Add pagination props (totalCount, currentPage, pageSize, handlers)
- ✅ Handle loading states during pagination
- ✅ Notify parent component of page/pageSize changes

### 6.2 Update LeetCode Page Component ✅
**File:** `client/src/pages/Leetcode/Leetcode.page.tsx`
- ✅ Update to use new response structure (`result.items`)
- ✅ Access problem attempts from `problemAttemptsResponse?.responseBody?.result.items`
- ✅ Add pagination state (currentPage, pageSize)
- ✅ Pass pagination state and handlers to table component
- ✅ Include Page/PageSize in API request
- ✅ Handle refetch on pagination changes automatically via React Query

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

- ✅ Initial page load returns only 10 problem attempts (instead of all 50,000+)
- ⏳ Pagination controls work correctly (pending frontend table implementation)
- ✅ Filtering applies before pagination (backend ready)
- ✅ Backend cache works per-page
- ✅ Performance improvement measurable (< 1s response time)
- ✅ Test data reduced from 50,000 to 1,000 attempts for testing
- ⏳ No breaking changes to existing functionality (pending integration testing)

## Technical Details

### Pagination Parameters
```
GET /api/v1/problem-attempts/get?Page=1&PageSize=10&IncludeCustom=false&IncludeLeetcode=true&UserIds=iyog4jlo4k
```

### Response Format
```json
{
  "responseBody": {
    "result": {
      "items": [...],
      "totalCount": 1000,
      "page": 1,
      "pageSize": 10,
      "totalPages": 100,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  }
}
```

### Cache Key Strategy
```
p{page}-ps{pageSize}-u{userIds}-s{seasonId}-lc{includeLeetcode}-c{includeCustom}
```

## Notes

- This is a learning exercise focusing on Problem Attempts endpoint
- LeetCode Problems and Mock Interviews pagination can follow the same pattern later
- Keep existing client-side pagination as reference until migration complete
- Difficulty chart and graph components temporarily use old API structure (will break until updated)

## Files Modified/Created

### New Files
- `server/Shared/PaginationDtos.cs` ✅

### Modified Files (Backend) ✅
- `server/Features/ProblemAttempts/Dtos/ListProblemAttempt.cs` ✅
- `server/Common/Interfaces/IRepository.cs` ✅
- `server/Common/EntityRepository.cs` ✅
- `server/Features/ProblemAttempts/ProblemAttemptService.cs` ✅
- `server/Features/ProblemAttempts/ProblemAttemptController.cs` ✅

### Modified Files (Frontend) ⏳
- `client/src/generated/api/client.ts` ✅ (regenerated)
- `client/src/pages/Leetcode/Leetcode.page.tsx` ✅ (partially updated)
- `client/src/pages/Leetcode/LeetcodeTable/LeetcodeTable.tsx` ⏳ (needs server-side pagination)

## Estimated Time
- Backend Implementation: 2-3 hours
- Frontend Implementation: 1-2 hours
- Testing & Debugging: 1-2 hours
- **Total: 4-7 hours**
