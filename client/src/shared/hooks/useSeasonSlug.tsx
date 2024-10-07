import { useLocation } from 'react-router-dom';

export const useSeasonSlug = () => {
  const location = useLocation();
  const pathSegments = location.pathname.split('/').filter(Boolean);
  let seasonIndex = pathSegments.findIndex((segment) => segment === 'seasons');
  const seasonSlug =
    seasonIndex !== -1 && seasonIndex + 1 < pathSegments.length
      ? pathSegments[seasonIndex + 1]
      : '';

  // Parse full url if there is no season
  if (seasonSlug === '') {
    seasonIndex = 0;
  }

  return { seasonSlug, pathSegments };
};
