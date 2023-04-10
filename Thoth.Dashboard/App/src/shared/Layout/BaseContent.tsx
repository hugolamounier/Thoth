import React from 'react';
import { Content } from 'antd/lib/layout/layout';

interface BaseContentProps {
  title: React.ReactNode;
  children: React.ReactNode;
}

const BaseContent = ({ children, title }: BaseContentProps): JSX.Element => (
  <Content
    className="p-6 py-4"
    style={{
      overflowY: 'auto',
    }}
  >
    <div
      className="shadow-1"
      style={{
        padding: 24,
        margin: '1rem 0 0 0',
        minHeight: 280,
        borderRadius: '0.25rem',
        backgroundColor: 'white',
      }}
    >
      {title}
      {children}
    </div>
  </Content>
);

export default BaseContent;
