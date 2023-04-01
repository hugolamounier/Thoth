import React from 'react';
import './index.css';
import BaseLayout from './shared/Layout/BaseLayout';
import FeatureFlags from './pages/featureFlags';
import { App as AppAntd } from 'antd';

function App() {
  return (
    <AppAntd>
      <BaseLayout>
        {/*<AppRoutes />*/}
        <FeatureFlags />
      </BaseLayout>
    </AppAntd>
  );
}

export default App;
