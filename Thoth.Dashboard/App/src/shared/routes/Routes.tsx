import { BrowserRouter, Route, Routes } from 'react-router-dom';
import FeatureManagement from '../../pages/featureFlags';

export default function AppRoutes(): JSX.Element {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<FeatureManagement />} path="/" />
        <Route element={<FeatureManagement />} path="/index.html" />
      </Routes>
    </BrowserRouter>
  );
}
