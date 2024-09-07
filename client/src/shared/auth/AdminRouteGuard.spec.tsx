import { render, screen } from '@test-utils';
import { describe, expect, it, vi } from 'vitest';
import AuthRouteGuard from './AuthRouteGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NavigateMock = () => <div data-testid="navigate">Navigate</div>;

const mocks = vi.hoisted(() => ({
  useAuth0: vi.fn(),
}));

vi.mock('@auth0/auth0-react', () => ({
  useAuth0: mocks.useAuth0,
}));

vi.mock('react-router-dom', () => ({
  useLocation: vi.fn(),
  useNavigate: vi.fn(),
  Navigate: vi.fn(() => NavigateMock),
  Outlet: vi.fn(() => OutletMock),
}));

describe('AuthRouteGuard', () => {
  it('renders null when loading', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
    });
    render(<AuthRouteGuard />);
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeNull();
  });

  it('renders Outlet when not loading and authenticated', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
    });
    render(<AuthRouteGuard />);
    expect(screen.queryByTestId('outlet')).toBeDefined();
    expect(screen.queryByTestId('navigate')).toBeNull();
  });

  it('displays NotFound when not loading and not authenticated', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
    });
    render(<AuthRouteGuard />);
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeNull();
    expect(screen.findAllByText('Page Not Found')).toBeDefined();
  });
});
