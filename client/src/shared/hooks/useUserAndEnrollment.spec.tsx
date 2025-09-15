import { renderHook } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useUserAndEnrollment } from './useUserAndEnrollment';

const mocks = vi.hoisted(() => ({
  useGetCurrentUser: vi.fn(),
  useGetIsUserEnrolled: vi.fn(),
  useGetUser: vi.fn(),
  useAuth0User: vi.fn(),
}));

vi.mock('@/generated/api/client', () => ({
  useGetCurrentUser: mocks.useGetCurrentUser,
  useGetIsCurrentUserEnrolled: mocks.useGetIsUserEnrolled,
  useGetUser: mocks.useGetUser,
}));

vi.mock('./useAuth0User', () => ({
  useAuth0User: mocks.useAuth0User,
}));

describe('useUserAndEnrollment', () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it('returns loading state when fetching data', () => {
    mocks.useAuth0User.mockReturnValue({
      isAdmin: false,
    });

    mocks.useGetCurrentUser.mockReturnValue({
      data: null,
      isLoading: true,
      isFetching: true,
      isError: false,
    });

    mocks.useGetIsUserEnrolled.mockReturnValue({
      data: null,
      isLoading: true,
      isFetching: true,
      isError: false,
    });

    mocks.useGetUser.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    const { result } = renderHook(() => useUserAndEnrollment('test-season-slug'));

    expect(result.current.isLoading).toBe(true);
    expect(result.current.isError).toBe(false);
    expect(result.current.user).toBeUndefined();
    expect(result.current.role).toBeNull();
  });

  it('returns error state when there is an error', () => {
    mocks.useAuth0User.mockReturnValue({
      isAdmin: false,
    });

    mocks.useGetCurrentUser.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: true,
    });

    mocks.useGetIsUserEnrolled.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: true,
    });

    mocks.useGetUser.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    const { result } = renderHook(() => useUserAndEnrollment('test-season-slug'));

    expect(result.current.isLoading).toBe(false);
    expect(result.current.isError).toBe(true);
    expect(result.current.user).toBeUndefined();
    expect(result.current.role).toBeNull();
  });

  it('returns user and admin status when data is available', () => {
    mocks.useAuth0User.mockReturnValue({
      isAdmin: true,
    });

    mocks.useGetCurrentUser.mockReturnValue({
      data: {
        responseBody: {
          user: {
            userId: 'test-user-id',
            name: 'Test User',
            slug: 'test-user',
            email: 'test@example.com',
          },
        },
      },
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    mocks.useGetIsUserEnrolled.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: false,
    });
    // When no email is provided, useGetUser is disabled.
    mocks.useGetUser.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    const { result } = renderHook(() => useUserAndEnrollment('test-season-slug'));

    expect(result.current.isLoading).toBe(false);
    expect(result.current.isError).toBe(false);
    // Fallback to the current user data from useGetCurrentUser
    expect(result.current.user).toEqual({
      userId: 'test-user-id',
      name: 'Test User',
      slug: 'test-user',
      email: 'test@example.com',
    });
    expect(result.current.isAdmin).toBe(true);
    expect(result.current.role).toBeNull();
  });

  it('returns role and enrollment ID when enrolled', () => {
    mocks.useAuth0User.mockReturnValue({
      isAdmin: false,
    });

    mocks.useGetCurrentUser.mockReturnValue({
      data: {
        responseBody: {
          user: {
            userId: 'test-user-id',
            name: 'Test User',
            slug: 'test-user',
            email: 'test@example.com',
          },
        },
      },
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    mocks.useGetIsUserEnrolled.mockReturnValue({
      data: {
        responseBody: {
          role: 'student',
          enrollmentId: 'test-enrollment-id',
        },
      },
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    // When no email is provided, useGetUser is disabled.
    mocks.useGetUser.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    const { result } = renderHook(() => useUserAndEnrollment('test-season-slug'));

    expect(result.current.isLoading).toBe(false);
    expect(result.current.isError).toBe(false);
    expect(result.current.user).toEqual({
      userId: 'test-user-id',
      name: 'Test User',
      slug: 'test-user',
      email: 'test@example.com',
    });
    expect(result.current.isAdmin).toBe(false);
    expect(result.current.role).toBe('student');
    expect(result.current.enrollmentId).toBe('test-enrollment-id');
  });

  it('supports querying with an email', () => {
    mocks.useAuth0User.mockReturnValue({
      isAdmin: false,
    });

    mocks.useGetCurrentUser.mockReturnValue({
      data: {
        responseBody: {
          user: {
            userId: 'test-user-id',
            name: 'Test User',
            slug: 'test-user',
            email: 'test@example.com',
          },
        },
      },
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    mocks.useGetIsUserEnrolled.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    // Even when querying with an email, if useGetUser returns no data we fallback.
    mocks.useGetUser.mockReturnValue({
      data: null,
      isLoading: false,
      isFetching: false,
      isError: false,
    });

    const { result } = renderHook(() => useUserAndEnrollment('test-season-slug', 'user@gmail.com'));

    expect(result.current.isLoading).toBe(false);
    expect(result.current.isError).toBe(false);
    // Fallback to the current user data if useGetUser data is not available.
    expect(result.current.user).toEqual({
      userId: 'test-user-id',
      name: 'Test User',
      slug: 'test-user',
      email: 'test@example.com',
    });
    expect(result.current.isAdmin).toBe(false);
    expect(result.current.role).toBeNull();
  });
});
