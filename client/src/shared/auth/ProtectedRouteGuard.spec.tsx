import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import ProtectedRouteGuard from './ProtectedRouteGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NavigateMock = () => <div data-testid="navigate">Navigate</div>;

const dummyLocation = {
  pathname: '/test-route',
  search: '',
  hash: '',
  state: null,
  key: 'test',
};

const mocks = vi.hoisted(() => ({
  useAuth0: vi.fn(),
}));

vi.mock('@auth0/auth0-react', () => ({
  useAuth0: mocks.useAuth0,
}));

vi.mock('react-router-dom', () => ({
  useLocation: vi.fn(() => dummyLocation),
  Navigate: vi.fn(() => NavigateMock),
  Outlet: vi.fn(() => OutletMock),
}));

describe('ProtectedRouteGuard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.resetAllMocks();
  });

  it('renders Outlet when user is authenticated and not loading', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
    });
    render(<ProtectedRouteGuard />);

    expect(screen.queryByTestId('outlet')).toBeDefined();
    expect(screen.queryByTestId('navigate')).toBeNull();
  });

  it('redirects to /login when user is not authenticated', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
    });

    render(<ProtectedRouteGuard />);

    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeDefined();
  });

  it('redirects to /login when user authentication is still loading', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
    });

    render(<ProtectedRouteGuard />);

    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeDefined();
  });
});
