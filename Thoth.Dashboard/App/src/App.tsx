import React from 'react';
import './index.css';
import BaseLayout from './shared/Layout/BaseLayout';
import AppRoutes from './shared/routes/Routes';

function App() {
  return (
    <BaseLayout>
      <AppRoutes />
    </BaseLayout>
  );
}

export default App;
