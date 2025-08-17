import { render, screen } from '@test-utils';
import { describe, expect, it, vi } from 'vitest';
import VerifiedUserAuthGuard from './VerifiedUserAuthGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NavigateMock = () => <div data-testid="navigate">Navigate</div>;
const LinkMock = () => <div data-testid="link">Link</div>;

const mocks = vi.hoisted(() => ({
  useAuth0: vi.fn(),
}));

vi.mock('@auth0/auth0-react', () => ({
  useAuth0: mocks.useAuth0,
}));

vi.mock('react-router-dom', () => ({
  useLocation: vi.fn(),
  useNavigate: vi.fn(),
  Navigate: () => <NavigateMock />,
  Outlet: () => <OutletMock />,
  Link: () => <LinkMock />,
}));

describe('VerifiedUserAuthGuard', () => {
  it('renders null when loading', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
    });

    render(<VerifiedUserAuthGuard />);
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders Outlet when authenticated and email verified', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: { email_verified: true },
    });

    render(<VerifiedUserAuthGuard />);
    expect(screen.getByTestId('outlet')).toBeInTheDocument();
    expect(screen.queryByTestId('navigate')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('navigates to verify email when user is not verified', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: { email_verified: false },
    });

    render(<VerifiedUserAuthGuard />);
    expect(screen.getByTestId('navigate')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders NotFound when not authenticated and not loading', () => {
    mocks.useAuth0.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      user: null,
    });

    render(<VerifiedUserAuthGuard />);
    expect(screen.findAllByText('Page Not Found')).toBeDefined();
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeNull();
  });
});
