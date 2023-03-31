import React, { useEffect, useState } from 'react';
import BaseContent from '../../shared/Layout/BaseContent';
import { Button, Modal, Space, Table, Tag } from 'antd';
import { DeleteOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { FeatureFlag, FeatureFlagsTypes } from '../../models/featureFlag';
import FeatureFlagService from '../../services/featureFlagService';
import moment from 'moment';

const FeatureFlags = (): JSX.Element => {
  const [featureFlags, setFeatureFlags] = useState<FeatureFlag[]>([]);
  const [loading, setLoading] = useState<boolean>(true);

  const [modal, contextHolder] = Modal.useModal();

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
    modal.confirm({
      title: 'Are you sure?',
      icon: <ExclamationCircleOutlined />,
      content: deleteMessage,
      okText: 'Cancel',
      cancelText: 'Delete',
      onCancel: async () => await deleteFlag(name),
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

  const tagValue = (name: string, value: boolean) => {
    if (value)
      return (
        <Tag className="cursor-pointer" color="green" onClick={() => onValueClick(name)}>
          Enabled
        </Tag>
      );

    return (
      <Tag className="cursor-pointer" color="red" onClick={() => onValueClick(name)}>
        Disabled
      </Tag>
    );
  };

  const actions = (name: string) => (
    <Button type="primary" danger onClick={() => confirmDelete(name)}>
      <Space>
        <DeleteOutlined />
        <span>Delete</span>
      </Space>
    </Button>
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
      type: tagType(featureFlag.type),
      value: tagValue(featureFlag.name, featureFlag.value),
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
      {contextHolder}
    </BaseContent>
  );
};

export default FeatureFlags;
