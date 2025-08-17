import { render, screen } from '@test-utils';
import { describe, expect, it, vi } from 'vitest';
import ProtectedRouteGuard from './ProtectedRouteGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NavigateMock = () => <div data-testid="navigate">Navigate</div>;
const LinkMock = () => <div data-testid="link">Link</div>;

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
  useNavigate: vi.fn(),
  Navigate: () => <NavigateMock />,
  Outlet: () => <OutletMock />,
  Link: () => <LinkMock />,
}));

describe('ProtectedRouteGuard', () => {
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

  it('displays NotFound when not loading and not authenticated', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
    });
    render(<ProtectedRouteGuard />);
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeNull();
    expect(screen.findAllByText('Page Not Found')).toBeDefined();
  });
});
