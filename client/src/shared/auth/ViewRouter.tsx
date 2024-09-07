import { useRouteLoaderData } from 'react-router-dom';
import { SeasonRole, SeasonRoleViews } from '../Season';

interface ViewRouterProps {
  views: SeasonRoleViews;
}

const ViewRouter = ({ views }: ViewRouterProps) => {
  const role = useRouteLoaderData('season') as SeasonRole;
  const { coordinator: CoordinatorView, mentor: MentorView, student: StudentView } = views;

  if (role === SeasonRole.Coordinator) {
    return <CoordinatorView />;
  }
  if (role === SeasonRole.Mentor) {
    return <MentorView />;
  }
  return <StudentView />;
};

export default ViewRouter;
