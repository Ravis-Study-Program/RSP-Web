import { render, screen } from '@test-utils';
import * as ReactRouterDom from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { SeasonRole } from '../Season';
import SeasonRouteGuard from './SeasonRouteGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NavigateMock = () => <div data-testid="navigate">Navigate</div>;

const dummyLocation = {
  pathname: '/test-route',
  search: '',
  hash: '',
  state: null,
  key: 'test',
};

vi.mock('react-router-dom', () => ({
  useLoaderData: vi.fn(),
  useLocation: vi.fn(),
  useNavigate: vi.fn(),
  Navigate: vi.fn(() => NavigateMock),
  Outlet: vi.fn(() => OutletMock),
}));

describe('SeasonRouteGuard', () => {
  it('renders Outlet for non-Unregistered roles', () => {
    vi.spyOn(ReactRouterDom, 'useLoaderData').mockReturnValue(SeasonRole.Mentor);
    vi.spyOn(ReactRouterDom, 'useLocation').mockReturnValue(dummyLocation);

    render(<SeasonRouteGuard />);

    expect(screen.queryByTestId('outlet')).toBeDefined();
    expect(screen.queryByTestId('navigate')).toBeNull();
  });

  it('redirects to /seasons for Unregistered role', () => {
    vi.spyOn(ReactRouterDom, 'useLoaderData').mockReturnValue(SeasonRole.Unregistered);
    vi.spyOn(ReactRouterDom, 'useLocation').mockReturnValue(dummyLocation);

    render(<SeasonRouteGuard />);

    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('navigate')).toBeDefined();
  });
});
