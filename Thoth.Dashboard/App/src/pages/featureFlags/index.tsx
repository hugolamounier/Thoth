import React, { useEffect, useState } from 'react';
import BaseContent from '../../shared/Layout/BaseContent';
import { App, Button, Space, Switch, Table, Tag } from 'antd';
import { DeleteOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { FeatureFlag, FeatureFlagsTypes } from '../../models/featureFlag';
import FeatureFlagService from '../../services/featureFlagService';
import moment from 'moment';

const FeatureFlags = (): JSX.Element => {
  const [featureFlags, setFeatureFlags] = useState<FeatureFlag[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const { modal } = App.useApp();

  const deleteFlag = async (name: string) => {
    if (await FeatureFlagService.Delete(name)) await getFeatureFlags();
  };

  const confirmDelete = (name: string) => {
    const deleteMessage = (
      <span>
        Are you sure that you want to delete the feature flag: <b>'{name}'</b> ? <br />
        <br /> This can cause <b className="text-red-700">several issues</b> if you don't remove all
        dependencies your application have on this flag.
      </span>
    );
    const modalState = modal.confirm({
      title: 'Are you sure?',
      icon: <ExclamationCircleOutlined />,
      content: deleteMessage,
      footer: (
        <Space className="p-3 flex justify-end" style={{ width: '100%' }}>
          <Button onClick={async () => await deleteFlag(name)}>Delete</Button>
          <Button type="primary" onClick={() => modalState.destroy()}>
            Cancel
          </Button>
        </Space>
      ),
    });
  };

  const tagType = (type: FeatureFlagsTypes) => {
    switch (type) {
      case FeatureFlagsTypes.Boolean:
        return <Tag color="gold">{FeatureFlagsTypes[type]}</Tag>;

      case FeatureFlagsTypes.PercentageFilter:
        return <Tag color="blue">Percentage Filter</Tag>;

      default:
        return <Tag color="red">Unknown</Tag>;
    }
  };

  const onValueClick = async (name: string) => {
    const featureFlag = featureFlags.findIndex((x) => x.name === name);
    const newFeatureFlags = [...featureFlags];
    newFeatureFlags[featureFlag].value = !featureFlags[featureFlag].value;

    setFeatureFlags(newFeatureFlags);
    const response = await FeatureFlagService.Update(newFeatureFlags[featureFlag]);

    if (!response) {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      const oldFeatureFlags = [...featureFlags];
      oldFeatureFlags[featureFlag].value = !oldFeatureFlags[featureFlag].value;
      setFeatureFlags(oldFeatureFlags);
    }
  };

  const actions = (name: string) => (
    <Button type="primary" danger onClick={() => confirmDelete(name)}>
      <Space>
        <DeleteOutlined className="p-0 m-0" />
        <span>Delete</span>
      </Space>
    </Button>
  );

  const tableHeader: any[] = [
    { title: 'Name', key: 'name', dataIndex: 'name' },
    { title: 'Flag Type', key: 'type', dataIndex: 'type' },
    { title: 'State', key: 'value', dataIndex: 'value' },
    { title: 'Created At', key: 'createdAt', dataIndex: 'createdAt' },
    { title: 'Updated At', key: 'updatedAt', dataIndex: 'updatedAt' },
    { title: 'Actions', key: 'actions', dataIndex: 'actions' },
  ];

  const tableData = featureFlags?.map((featureFlag) => {
    return {
      key: featureFlag.name,
      name: featureFlag.name,
      type: tagType(featureFlag.type),
      value: (
        <Switch
          checkedChildren="On"
          unCheckedChildren="Off"
          checked={featureFlag.value}
          onChange={() => onValueClick(featureFlag.name)}
        />
      ),
      createdAt: moment(featureFlag.createdAt).format('YYYY-MM-DD hh:mm:ss'),
      updatedAt:
        featureFlag.updatedAt !== null
          ? moment(featureFlag.createdAt).format('YYYY-MM-DD hh:mm:ss')
          : '--',
      actions: actions(featureFlag.name),
    };
  });

  const getFeatureFlags = async () => {
    setLoading(true);
    const data = await FeatureFlagService.GetAll();
    setFeatureFlags(data);
    setLoading(false);
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
