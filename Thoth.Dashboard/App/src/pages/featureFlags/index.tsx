import React, { useEffect, useState } from 'react';
import BaseContent from '../../shared/Layout/BaseContent';
import { Button, notification, Space, Table } from 'antd';
import { DeleteOutlined } from '@ant-design/icons';
import { FeatureFlag, FeatureFlagsTypes } from '../../models/featureFlag';
import FeatureFlagService from '../../services/featureFlagService';
import moment from 'moment';

const FeatureFlags = (): JSX.Element => {
  const [featureFlags, setFeatureFlags] = useState<FeatureFlag[]>([]);
  const [loading, setLoading] = useState<boolean>(true);

  const actions = (
    <Space>
      <Button icon={<DeleteOutlined />} type="primary" danger>
        Delete
      </Button>
    </Space>
  );

  const tableHeader: any[] = [
    { title: 'Name', key: 'name', dataIndex: 'name' },
    { title: 'Type', key: 'type', dataIndex: 'type' },
    { title: 'Value', key: 'value', dataIndex: 'value' },
    { title: 'CreatedAt', key: 'createdAt', dataIndex: 'createdAt' },
    { title: 'UpdatedAt', key: 'updatedAt', dataIndex: 'updatedAt' },
    { title: 'Actions', key: 'actions', dataIndex: 'actions' },
  ];

  const tableData = featureFlags?.map((featureFlag) => {
    return {
      key: featureFlag.name,
      name: featureFlag.name,
      type: FeatureFlagsTypes[featureFlag.type],
      value: featureFlag.value,
      createdAt: moment(featureFlag.createdAt).format(),
      updatedAt:
        featureFlag.updatedAt !== undefined ? moment(featureFlag.updatedAt).format() : '--',
    };
  });

  const getFeatureFlags = async () => {
    setLoading(true);
    const data = await FeatureFlagService.GetAll();
    setFeatureFlags(data);
  };

  useEffect(() => {
    getFeatureFlags().finally(() => setLoading(false));
  }, []);

  return (
    <BaseContent title="Feature Flags">
      <Table loading={loading} columns={tableHeader} dataSource={tableData} />
    </BaseContent>
  );
};

export default FeatureFlags;
