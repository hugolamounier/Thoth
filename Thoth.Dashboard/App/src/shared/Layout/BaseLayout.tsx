import React, { ReactNode, useContext, useEffect } from 'react';
import { UnorderedListOutlined } from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { Layout, Menu, theme } from 'antd';
import { AppContext } from '../Contexts/AppContext';
import FeatureManager from '../../pages/featureManager';

const { Header, Sider } = Layout;

const BaseLayout = ({ children }: { children: ReactNode }): JSX.Element => {
  const { currentPage, setCurrentPage } = useContext(AppContext);
  const {
    token: { colorBgContainer },
  } = theme.useToken();

  useEffect(() => {
    console.log(currentPage);
  }, []);

  const items2: MenuProps['items'] = [
    {
      key: 1,
      label: 'Feature Management',
      icon: <UnorderedListOutlined />,
      children: [
        {
          key: 'activeFeatures',
          label: 'Active Features',
          onClick: () => {
            setCurrentPage(<FeatureManager listingFeatures="active" />);
          },
        },
        {
          key: 'deletedFeatures',
          label: 'Deleted Features',
          onClick: () => {
            setCurrentPage(<FeatureManager listingFeatures="deleted" />);
          },
        },
      ],
    },
  ];

  return (
    <Layout style={{ height: '100vh' }}>
      <Header className="flex align-items-center">
        <span className="text-logo text-white">Thoth</span>
      </Header>
      <Layout>
        <Sider className="shadow-1" width={230} style={{ background: colorBgContainer }}>
          <Menu
            className="pt-4 px-2"
            mode="inline"
            defaultSelectedKeys={['activeFeatures']}
            defaultOpenKeys={['1']}
            style={{ height: '100%', borderRight: 0 }}
            items={items2}
          />
        </Sider>
        <Layout>{children}</Layout>
      </Layout>
    </Layout>
  );
};

export default BaseLayout;
