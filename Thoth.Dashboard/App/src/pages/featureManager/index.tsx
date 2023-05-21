import React, { useEffect, useState } from 'react';
import BaseContent from '../../shared/Layout/BaseContent';
import { App, Button, Dropdown, MenuProps, Modal, Space, Switch, Table, Tooltip } from 'antd';
import {
  DeleteOutlined,
  EditOutlined,
  EllipsisOutlined,
  ExclamationCircleOutlined,
  HistoryOutlined,
  InfoCircleOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { FeatureManager, FeatureFlagsTypes, FeatureTypes } from '../../models/featureManager';
import FeatureFlagService from '../../services/featureFlagService';
import moment from 'moment';
import CreateModal from './components/createModal';
import Search from 'antd/es/input/Search';
import HistoryModal from './components/historyModal';
import TypeTagHelper from '../../shared/Helpers/TypeTagHelper';
import EditModal from './components/editModal';

type LoadingProps = {
  loading: boolean;
  updateLoading: Map<string, boolean>;
  createLoading: boolean;
  editLoading: boolean;
};

export type ModalOpenProps = {
  createModal: boolean;
  historyModal: boolean;
  editingModal: boolean;
};

type FeatureManagementProps = {
  listingFeatures: 'active' | 'deleted';
};

const FeatureManagement = ({ listingFeatures }: FeatureManagementProps): JSX.Element => {
  const [features, setFeatures] = useState<FeatureManager[]>([]);
  const [filteredFeatures, setFilteredFeatures] = useState<FeatureManager[]>();
  const [currentSearchValue, setCurrentSearchValue] = useState<string>('');
  const [modalOpen, setModalOpen] = useState<ModalOpenProps>({
    createModal: false,
    historyModal: false,
    editingModal: false,
  });
  const [loading, setLoading] = useState<LoadingProps>({
    loading: false,
    updateLoading: new Map<string, boolean>(),
    createLoading: false,
    editLoading: false,
  });
  const [currentFeatureHistory, setCurrentFeatureHistory] = useState<FeatureManager>();
  const [currentFeatureEditing, setCurrentFeatureEditing] = useState<FeatureManager>();

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

  const onSearchFeature = (search: string) => {
    if (search.length === 0) {
      setCurrentSearchValue('');
      setFilteredFeatures(features);
      return;
    }
    if (search.length >= 3) {
      setCurrentSearchValue(search);
      const filtered = features.filter((entry) => entry.name.toLowerCase().includes(search));
      setFilteredFeatures(filtered);
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

  const openFeatureHistory = (feature: FeatureManager) => {
    setCurrentFeatureHistory(feature);
    setModalOpen({ ...modalOpen, historyModal: true });
  };

  const openFeatureEditing = (feature: FeatureManager) => {
    setCurrentFeatureEditing(feature);
    setModalOpen({ ...modalOpen, editingModal: true });
  };

  const onSubmitCreateForm = async (data: FeatureManager) => {
    setLoading({ ...loading, createLoading: true });
    if (await FeatureFlagService.Create(data)) {
      await getFeatureFlags();
      setModalOpen({ ...modalOpen, createModal: false });
    }
    setLoading({ ...loading, createLoading: false });
  };

  const onSubmitEditingForm = async (data: FeatureManager, featureName: string) => {
    setLoading({ ...loading, editLoading: true });
    data.name = featureName;
    if (await FeatureFlagService.Update(data)) {
      await getFeatureFlags();
      setModalOpen({ ...modalOpen, editingModal: false });
    }
    setLoading({ ...loading, editLoading: false });
  };

  const onFeaturesChange = (newFeatures: FeatureManager[]) => {
    if (currentSearchValue.length > 0) onSearchFeature(currentSearchValue);
    if (currentSearchValue.length === 0) setFilteredFeatures(newFeatures);
  };

  const onValueClick = async (name: string) => {
    setLoading({ ...loading, updateLoading: loading.updateLoading.set(name, true) });
    const featureFlag = features.findIndex((x) => x.name === name);
    const newFeatureFlags = [...features];
    newFeatureFlags[featureFlag].enabled = !features[featureFlag].enabled;

    setFeatures(newFeatureFlags);
    onFeaturesChange(newFeatureFlags);

    const response = await FeatureFlagService.Update(newFeatureFlags[featureFlag]);

    if (!response) {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      const oldFeatureFlags = [...features];
      oldFeatureFlags[featureFlag].enabled = !oldFeatureFlags[featureFlag].enabled;
      setFeatures(oldFeatureFlags);
      onFeaturesChange(oldFeatureFlags);
      setLoading({ ...loading, updateLoading: loading.updateLoading.set(name, false) });
      return;
    }
    await getFeatureFlags();
    setLoading({ ...loading, updateLoading: loading.updateLoading.set(name, false) });
  };

  const actions = (feature: FeatureManager) => {
    const items: MenuProps['items'] = [
      {
        label: 'Edit',
        key: '2',
        icon: <EditOutlined />,
        onClick: () => openFeatureEditing(feature),
      },
      {
        label: 'Delete',
        key: '3',
        icon: <DeleteOutlined />,
        danger: true,
        onClick: () => confirmDelete(feature.name),
      },
    ];

    if (feature.description)
      items.unshift({
        label: 'History',
        key: '1',
        icon: <HistoryOutlined />,
        onClick: () => openFeatureHistory(feature),
      });

    return (
      <Dropdown.Button
        type="default"
        icon={<EllipsisOutlined className="rotate-90" />}
        menu={{ items }}
        trigger={['click']}
        onClick={() =>
          !feature.description
            ? openFeatureHistory(feature)
            : Modal.info({
                title: 'Feature Description',
                content: (
                  <Space direction="vertical">
                    <span>
                      <b>Name:</b> {feature.name}
                    </span>
                    <div className="break-all">{feature.description}</div>
                  </Space>
                ),
                footer: null,
                closable: true,
                width: 600,
              })
        }
      >
        {feature.description ? (
          <Tooltip title="Feature description">
            <InfoCircleOutlined className="icon text-yellow-800" />
          </Tooltip>
        ) : (
          <Tooltip title="See History">
            <HistoryOutlined className="icon" />
          </Tooltip>
        )}
      </Dropdown.Button>
    );
  };

  const tableHeader: any[] = [
    { title: 'Name', key: 'name', dataIndex: 'name' },
    { title: 'Flag Type', key: 'type', dataIndex: 'type' },
    { title: 'State', key: 'enabled', dataIndex: 'enabled' },
    { title: 'Value', key: 'value', dataIndex: 'value' },
    { title: 'Created At', key: 'createdAt', dataIndex: 'createdAt' },
    {
      title: listingFeatures === 'active' ? 'Updated At' : 'DeletedAt',
      key: listingFeatures === 'active' ? 'updatedAt' : 'updatedAt',
      dataIndex: listingFeatures === 'active' ? 'updatedAt' : 'deletedAt',
    },
    { title: 'Actions', key: 'actions', dataIndex: 'actions' },
  ];

  const tableData = filteredFeatures?.map((feature) => {
    return {
      key: feature.name,
      name: feature.name,
      type: TypeTagHelper.TagType(feature.type, feature.subType),
      enabled: (
        <Switch
          disabled={feature.type === FeatureTypes.EnvironmentVariable}
          checkedChildren="On"
          unCheckedChildren="Off"
          checked={feature.enabled}
          loading={loading.updateLoading?.get(feature.name) ?? false}
          onChange={() => onValueClick(feature.name)}
        />
      ),
      value: feature.value ?? '--',
      createdAt: moment(feature.createdAt).format('YYYY-MM-DD HH:mm:ss'),
      updatedAt:
        feature.updatedAt !== null ? moment(feature.updatedAt).format('YYYY-MM-DD HH:mm:ss') : '--',
      deletedAt: moment(feature.deletedAt).format('YYYY-MM-DD HH:mm:ss'),
      actions: actions(feature),
    };
  });

  const titleHeader = (
    <Space
      className="border-black border-b-2 pb-3 flex align-items-center justify-between"
      style={{ width: '100%' }}
    >
      <h1 className="text-heading-bold-4 ">Feature Management</h1>
      <Button type="primary" onClick={() => setModalOpen({ ...modalOpen, createModal: true })}>
        <Space>
          <PlusOutlined />
          <span>Create</span>
        </Space>
      </Button>
    </Space>
  );

  const getFeatureFlags = async () => {
    let data: FeatureManager[] = [];

    if (listingFeatures === 'active') data = await FeatureFlagService.GetAll();
    if (listingFeatures === 'deleted') data = await FeatureFlagService.GetAllDeleted();

    setFeatures(data);
    onFeaturesChange(data);
  };

  useEffect(() => {
    Modal.info({}).destroy();
    setLoading({ ...loading, loading: true });
    getFeatureFlags().finally(() => setLoading({ ...loading, loading: false }));
  }, []);

  return (
    <BaseContent title={titleHeader}>
      <Space className="w-full" direction="vertical">
        <Search
          type="default"
          className="my-2 w-1/2"
          onChange={(e) => onSearchFeature(e.target.value.toLowerCase())}
          placeholder="Search for feature by name"
          size="large"
        />
        <Table loading={loading.loading} columns={tableHeader} dataSource={tableData} />
      </Space>
      <CreateModal
        isOpen={modalOpen.createModal}
        setIsOpen={(state: boolean) => setModalOpen({ ...modalOpen, createModal: state })}
        onSubmitForm={onSubmitCreateForm}
        isLoading={loading.createLoading}
      />
      <HistoryModal
        isOpen={modalOpen.historyModal}
        setIsOpen={(state: boolean) => setModalOpen({ ...modalOpen, historyModal: state })}
        feature={currentFeatureHistory}
      />
      <EditModal
        isOpen={modalOpen.editingModal}
        setIsOpen={(state: boolean) => setModalOpen({ ...modalOpen, editingModal: state })}
        onSubmitForm={(data) => onSubmitEditingForm(data, currentFeatureEditing!.name)}
        isLoading={loading.editLoading}
        feature={currentFeatureEditing}
      />
    </BaseContent>
  );
};

export default FeatureManagement;
