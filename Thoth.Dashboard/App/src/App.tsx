import React from 'react';
import './index.css';
import BaseLayout from './shared/Layout/BaseLayout';
import AppRoutes from './shared/routes/Routes';
import FeatureFlags from './pages/featureFlags';

function App() {
  return (
    <BaseLayout>
      {/*<AppRoutes />*/}
      <FeatureFlags />
    </BaseLayout>
  );
}

export default App;
