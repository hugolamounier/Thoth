import React, { useContext } from 'react';
import './index.css';
import BaseLayout from './shared/Layout/BaseLayout';
import { App as AppAntd } from 'antd';
import { AppContext } from './shared/Contexts/AppContext';

function App() {
  const { currentPage } = useContext(AppContext);
  return (
    <AppAntd>
      <BaseLayout>{currentPage}</BaseLayout>
    </AppAntd>
  );
}

export default App;
