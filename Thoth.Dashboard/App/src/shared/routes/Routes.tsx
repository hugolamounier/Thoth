import { BrowserRouter, Route, Routes } from 'react-router-dom';
import FeatureFlags from '../../pages/featureFlags';

export default function AppRoutes(): JSX.Element {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<FeatureFlags />} path="/" />
      </Routes>
    </BrowserRouter>
  );
}
