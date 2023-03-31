import React, { ReactNode } from 'react';
import { LaptopOutlined, NotificationOutlined, UserOutlined } from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { Breadcrumb, Layout, Menu, theme } from 'antd';

const { Header, Content, Sider } = Layout;

const items2: MenuProps['items'] = [{ key: 1, label: 'Feature Flags' }];

const BaseLayout = ({ children }: { children: ReactNode }): JSX.Element => {
  const {
    token: { colorBgContainer },
  } = theme.useToken();

  return (
    <Layout style={{ height: '100vh' }}>
      <Header className="flex align-items-center">
        <span className="text-logo text-white">Thoth</span>
      </Header>
      <Layout>
        <Sider width={230} style={{ background: colorBgContainer }}>
          <Menu
            className="pt-4 px-2"
            mode="inline"
            defaultSelectedKeys={['1']}
            style={{ height: '100%', borderRight: 0 }}
            items={items2}
          />
        </Sider>
        <Layout style={{ padding: '0 24px 24px' }}>
          <Content
            style={{
              padding: 24,
              margin: '1rem 0 0 0',
              minHeight: 280,
              background: colorBgContainer,
            }}
          >
            {children}
          </Content>
        </Layout>
      </Layout>
    </Layout>
  );
};

export default BaseLayout;
