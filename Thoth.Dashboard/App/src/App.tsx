import React from 'react';
import './index.css';
import BaseLayout from './shared/Layout/BaseLayout';
import FeatureManagement from './pages/featureFlags';
import { App as AppAntd } from 'antd';

function App() {
  return (
    <AppAntd>
      <BaseLayout>
        {/*<AppRoutes />*/}
        <FeatureManagement />
      </BaseLayout>
    </AppAntd>
  );
}

export default App;
