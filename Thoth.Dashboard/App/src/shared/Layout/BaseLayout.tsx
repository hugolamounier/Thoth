import React, { ReactNode } from 'react';
import { UnorderedListOutlined } from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { Layout, Menu, Space, theme } from 'antd';

const { Header, Sider } = Layout;

const items2: MenuProps['items'] = [
  { key: 1, label: 'Feature Management', icon: <UnorderedListOutlined /> },
];

const BaseLayout = ({ children }: { children: ReactNode }): JSX.Element => {
  const {
    token: { colorBgContainer },
  } = theme.useToken();

  return (
    <Layout style={{ height: '100vh' }}>
      <Header className="flex align-items-center">
        <Space>
          <span className="text-logo text-white">Thoth</span>
        </Space>
      </Header>
      <Layout>
        <Sider className="shadow-1" width={230} style={{ background: colorBgContainer }}>
          <Menu
            className="pt-4 px-2"
            mode="inline"
            defaultSelectedKeys={['1']}
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
