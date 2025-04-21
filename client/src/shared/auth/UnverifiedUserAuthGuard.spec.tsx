import { render, screen } from '@test-utils';
import { describe, expect, it, vi } from 'vitest';
import UnverifiedUserAuthGuard from './UnverifiedUserAuthGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NavigateMock = () => <div data-testid="navigate">Navigate</div>;
const NotFoundMock = () => <div data-testid="not-found">Not Found</div>;

const mocks = vi.hoisted(() => ({
  useAuth0: vi.fn(),
}));

vi.mock('@auth0/auth0-react', () => ({
  useAuth0: mocks.useAuth0,
}));

vi.mock('react-router-dom', () => ({
  Navigate: () => <NavigateMock />,
  Outlet: () => <OutletMock />,
}));

vi.mock('@/pages/NotFound/NotFound.page', () => ({
  __esModule: true,
  default: () => <NotFoundMock />,
}));

describe('UnverifiedUserAuthGuard', () => {
  it('renders null when auth is loading', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
      user: null,
    });

    render(<UnverifiedUserAuthGuard />);
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders NotFoundPage when not authenticated', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      user: null,
    });

    render(<UnverifiedUserAuthGuard />);
    expect(screen.getByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeNull();
  });

  it('navigates to "/" when authenticated and verified', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: { email_verified: true },
    });

    render(<UnverifiedUserAuthGuard />);
    expect(screen.getByTestId('navigate')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders Outlet when authenticated and not verified', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: { email_verified: false },
    });

    render(<UnverifiedUserAuthGuard />);
    expect(screen.getByTestId('outlet')).toBeInTheDocument();
    expect(screen.queryByTestId('navigate')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });
});
