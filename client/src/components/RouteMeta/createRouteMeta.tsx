// components/RouteMeta/createRouteMeta.tsx
import { Helmet } from 'react-helmet-async';
import { Route } from 'react-router-dom';

type CreateRouteMetaProps = {
  path?: string;
  index?: boolean;
  title: string;
  element: React.ReactNode;
};

export function createRouteMeta({ path, index = false, title, element }: CreateRouteMetaProps) {
  const wrapped = (
    <>
      <Helmet>
        <title>{title}</title>
      </Helmet>
      {element}
    </>
  );

  return <Route path={path} index={index} element={wrapped} />;
}
