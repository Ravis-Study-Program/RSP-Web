import { renderHook } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useSeasonSlug } from './useSeasonSlug';

const mocks = vi.hoisted(() => ({
  useLocation: vi.fn(),
}));

vi.mock('react-router-dom', () => ({
  useLocation: mocks.useLocation,
}));

describe('useSeasonSlug', () => {
  it('returns the correct seasonSlug and pathSegments when seasonSlug is present', () => {
    mocks.useLocation.mockReturnValue({
      pathname: '/seasons/nyc-2023-2024/leetcode',
    });

    const { result } = renderHook(() => useSeasonSlug());

    expect(result.current.seasonSlug).toBe('nyc-2023-2024');
    expect(result.current.pathSegments).toEqual(['seasons', 'nyc-2023-2024', 'leetcode']);
  });

  it('returns an empty seasonSlug and full pathSegments when seasonSlug is absent', () => {
    mocks.useLocation.mockReturnValue({
      pathname: '/leetcode',
    });

    const { result } = renderHook(() => useSeasonSlug());

    expect(result.current.seasonSlug).toBe('');
    expect(result.current.pathSegments).toEqual(['leetcode']);
  });

  it('handles root path correctly', () => {
    mocks.useLocation.mockReturnValue({
      pathname: '/',
    });

    const { result } = renderHook(() => useSeasonSlug());

    expect(result.current.seasonSlug).toBe('');
    expect(result.current.pathSegments).toEqual([]);
  });

  it('handles paths with multiple segments and no season correctly', () => {
    mocks.useLocation.mockReturnValue({
      pathname: '/users/profile/edit',
    });

    const { result } = renderHook(() => useSeasonSlug());

    expect(result.current.seasonSlug).toBe('');
    expect(result.current.pathSegments).toEqual(['users', 'profile', 'edit']);
  });

  it('handles paths with "seasons" but no subsequent segment', () => {
    mocks.useLocation.mockReturnValue({
      pathname: '/seasons',
    });

    const { result } = renderHook(() => useSeasonSlug());

    expect(result.current.seasonSlug).toBe('');
    expect(result.current.pathSegments).toEqual(['seasons']);
  });
});
