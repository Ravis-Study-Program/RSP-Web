import { renderHook } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useUserAndEnrollment } from './useUserAndEnrollment';

const mocks = vi.hoisted(() => ({
  useGetCurrentUser: vi.fn(),
  useGetIsUserEnrolled: vi.fn(),
  useGetUser: vi.fn(), // Added mock for useGetUser
}));

vi.mock('@/generated/api/client', () => ({
  useGetCurrentUser: mocks.useGetCurrentUser,
  useGetIsCurrentUserEnrolled: mocks.useGetIsUserEnrolled,
  useGetUser: mocks.useGetUser,
}));

describe('useUserAndEnrollment', () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it('returns loading state when fetching data', () => {
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
    mocks.useGetCurrentUser.mockReturnValue({
      data: {
        responseBody: {
          user: { id: 'test-user-id', isAdmin: true },
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
    expect(result.current.user).toEqual({ id: 'test-user-id', isAdmin: true });
    expect(result.current.isAdmin).toBe(true);
    expect(result.current.role).toBeNull();
  });

  it('returns role and enrollment ID when enrolled', () => {
    mocks.useGetCurrentUser.mockReturnValue({
      data: {
        responseBody: {
          user: { id: 'test-user-id', isAdmin: false },
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
    expect(result.current.user).toEqual({ id: 'test-user-id', isAdmin: false });
    expect(result.current.isAdmin).toBe(false);
    expect(result.current.role).toBe('student');
    expect(result.current.enrollmentId).toBe('test-enrollment-id');
  });

  it('supports querying with an email', () => {
    mocks.useGetCurrentUser.mockReturnValue({
      data: {
        responseBody: {
          user: { id: 'test-user-id', isAdmin: false, email: 'user@gmail.com' },
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
      id: 'test-user-id',
      isAdmin: false,
      email: 'user@gmail.com',
    });
    expect(result.current.isAdmin).toBe(false);
    expect(result.current.role).toBeNull();
  });
});
