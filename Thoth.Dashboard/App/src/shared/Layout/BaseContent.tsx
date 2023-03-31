import React from 'react';
import { Content } from 'antd/lib/layout/layout';

interface BaseContentProps {
  title: React.ReactNode;
  children: React.ReactNode;
}

const BaseContent = ({ children, title }: BaseContentProps): JSX.Element => (
  <Content
    className="shadow-1"
    style={{
      padding: 24,
      margin: '1rem 0 0 0',
      minHeight: 280,
      borderRadius: '0.25rem',
      backgroundColor: 'white',
    }}
  >
    <h1 className="pb-3 text-heading-bold-4 border-black border-b-2">{title}</h1>
    <div className="py-4">{children}</div>
  </Content>
);

export default BaseContent;
