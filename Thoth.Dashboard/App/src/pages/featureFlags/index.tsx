import React, { useEffect, useState } from 'react';
import BaseContent from '../../shared/Layout/BaseContent';
import { App, Button, Modal, Space, Switch, Table, Tag, Tooltip } from 'antd';
import {
  DeleteOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { FeatureFlag, FeatureFlagsTypes, FeatureTypes } from '../../models/featureFlag';
import FeatureFlagService from '../../services/featureFlagService';
import moment from 'moment';
import CreateModal from './components/createModal';

type LoadingProps = {
  loading: boolean;
  updateLoading: Map<string, boolean>;
  createLoading: boolean;
};

const FeatureManagement = (): JSX.Element => {
  const [featureFlags, setFeatureFlags] = useState<FeatureFlag[]>([]);
  const [createModalOpen, setCreateModalOpen] = useState<boolean>(false);
  const [loading, setLoading] = useState<LoadingProps>({
    loading: false,
    updateLoading: new Map<string, boolean>(),
    createLoading: false,
  });
  const { modal } = App.useApp();

  const deleteFlag = async (
    name: string,
    modalState: { destroy: () => void; update: (configUpdate: any) => void }
  ) => {
    if (await FeatureFlagService.Delete(name)) {
      await getFeatureFlags();
      modalState.destroy();
    }
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
          <Button onClick={async () => await deleteFlag(name, modalState)}>Delete</Button>
          <Button type="primary" onClick={() => modalState.destroy()}>
            Cancel
          </Button>
        </Space>
      ),
    });
  };

  const onSubmitForm = async (data: FeatureFlag) => {
    setLoading({ ...loading, createLoading: true });
    if (await FeatureFlagService.Create(data)) {
      await getFeatureFlags();
      setCreateModalOpen(false);
    }
    setLoading({ ...loading, createLoading: false });
  };

  const tagType = (type: FeatureTypes, subType?: FeatureFlagsTypes) => {
    switch (type) {
      case FeatureTypes.EnvironmentVariable:
        return <Tag color="gold">{FeatureTypes[type]}</Tag>;

      case FeatureTypes.FeatureFlag: {
        switch (subType) {
          case FeatureFlagsTypes.Boolean:
            return <Tag color="green">Feature Flag: {FeatureFlagsTypes[subType]}</Tag>;

          case FeatureFlagsTypes.PercentageFilter:
            return <Tag color="purple">Feature Flag: {FeatureFlagsTypes[subType]}</Tag>;
          default:
            return <Tag color="red">Feature Flag: Unknown</Tag>;
        }
      }
      default:
        return <Tag color="red">Unknown</Tag>;
    }
  };

  const onValueClick = async (name: string) => {
    setLoading({ ...loading, updateLoading: loading.updateLoading.set(name, true) });
    const featureFlag = featureFlags.findIndex((x) => x.name === name);
    const newFeatureFlags = [...featureFlags];
    newFeatureFlags[featureFlag].enabled = !featureFlags[featureFlag].enabled;

    setFeatureFlags(newFeatureFlags);
    const response = await FeatureFlagService.Update(newFeatureFlags[featureFlag]);

    if (!response) {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      const oldFeatureFlags = [...featureFlags];
      oldFeatureFlags[featureFlag].enabled = !oldFeatureFlags[featureFlag].enabled;
      setFeatureFlags(oldFeatureFlags);
      setLoading({ ...loading, updateLoading: loading.updateLoading.set(name, false) });
      return;
    }
    await getFeatureFlags();
    setLoading({ ...loading, updateLoading: loading.updateLoading.set(name, false) });
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
    { title: 'State', key: 'enabled', dataIndex: 'enabled' },
    { title: 'Value', key: 'value', dataIndex: 'value' },
    { title: 'Created At', key: 'createdAt', dataIndex: 'createdAt' },
    { title: 'Updated At', key: 'updatedAt', dataIndex: 'updatedAt' },
    { title: 'Actions', key: 'actions', dataIndex: 'actions' },
  ];

  const tableData = featureFlags?.map((featureFlag) => {
    return {
      key: featureFlag.name,
      name: (
        <Space align="center">
          {featureFlag.name}
          {featureFlag?.description !== null ? (
            <Tooltip className="pl-1 cursor-pointer" title="Description">
              <InfoCircleOutlined
                onClick={() =>
                  Modal.info({
                    title: `${featureFlag.name} - Description`,
                    content: <div>{featureFlag.description}</div>,
                    footer: null,
                    closable: true,
                  })
                }
              />
            </Tooltip>
          ) : null}
        </Space>
      ),
      type: tagType(featureFlag.type, featureFlag.subType),
      enabled: (
        <Switch
          checkedChildren="On"
          unCheckedChildren="Off"
          checked={featureFlag.enabled}
          loading={loading.updateLoading?.get(featureFlag.name) ?? false}
          onChange={() => onValueClick(featureFlag.name)}
        />
      ),
      value: featureFlag.value ?? '--',
      createdAt: moment(featureFlag.createdAt).format('YYYY-MM-DD HH:mm:ss'),
      updatedAt:
        featureFlag.updatedAt !== null
          ? moment(featureFlag.updatedAt).format('YYYY-MM-DD HH:mm:ss')
          : '--',
      actions: actions(featureFlag.name),
    };
  });

  const titleHeader = (
    <Space
      className="border-black border-b-2 pb-3 flex align-items-center justify-between"
      style={{ width: '100%' }}
    >
      <h1 className="text-heading-bold-4 ">Feature Management</h1>
      <Button type="primary" onClick={() => setCreateModalOpen(true)}>
        <Space>
          <PlusOutlined className="p-0 m-0" />
          <span>Create</span>
        </Space>
      </Button>
    </Space>
  );

  const getFeatureFlags = async () => {
    const data = await FeatureFlagService.GetAll();
    setFeatureFlags(data);
  };

  useEffect(() => {
    setLoading({ ...loading, loading: true });
    getFeatureFlags().finally(() => setLoading({ ...loading, loading: false }));
  }, []);

  return (
    <BaseContent title={titleHeader}>
      <Table loading={loading.loading} columns={tableHeader} dataSource={tableData} />
      <CreateModal
        isOpen={createModalOpen}
        setIsOpen={setCreateModalOpen}
        onSubmitForm={onSubmitForm}
        isLoading={loading.createLoading}
      />
    </BaseContent>
  );
};

export default FeatureManagement;
