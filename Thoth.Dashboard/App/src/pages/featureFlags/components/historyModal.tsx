import React, { useEffect } from 'react';
import { Modal, Space, Switch, Table, Tag, Tooltip } from 'antd';
import { HistoryOutlined, InfoCircleOutlined } from '@ant-design/icons';
import { FeatureManager, FeatureTypes } from '../../../models/featureManager';
import moment from 'moment/moment';
import TypeTagHelper from '../../../shared/Helpers/TypeTagHelper';

interface HistoryModalInterface {
  isOpen: boolean;
  setIsOpen: (state: boolean) => void;
  feature?: FeatureManager;
}

const HistoryModal = ({ isOpen, setIsOpen, feature }: HistoryModalInterface) => {
  const tableHeader: any[] = [
    { title: 'State', key: 'enabled', dataIndex: 'enabled' },
    { title: 'Value', key: 'value', dataIndex: 'value' },
    { title: 'Period Start', key: 'periodStart', dataIndex: 'periodStart' },
    { title: 'Period End', key: 'periodEnd', dataIndex: 'periodEnd' },
    { title: 'Audit Extras', key: 'extras', dataIndex: 'extras', width: 500 },
  ];

  const getTableData = () => {
    const tableData = feature?.histories?.map((feature) => {
      return {
        key: feature.name,
        current: false,
        enabled: (
          <Space>
            <Tag className="text-gray-500 border-gray-500" color="#f3f4f6">
              Obsolete
            </Tag>
            <Switch
              disabled={true}
              checkedChildren="On"
              unCheckedChildren="Off"
              checked={feature.enabled}
            />
          </Space>
        ),
        value: feature.value ?? '--',
        periodEnd: moment(feature.periodEnd).format('YYYY-MM-DD HH:mm:ss'),
        periodStart: moment(feature.periodStart).format('YYYY-MM-DD HH:mm:ss'),
        extras: feature.extras ? (
          <div className="break-all">{JSON.stringify(JSON.parse(feature.extras), null, 10)}</div>
        ) : (
          '--'
        ),
      };
    });

    if (tableData && feature) {
      tableData.unshift({
        key: feature.name,
        current: true,
        enabled: (
          <Space>
            <Tag color="green">Current</Tag>
            <Switch
              disabled={true}
              checkedChildren="On"
              unCheckedChildren="Off"
              checked={feature.enabled}
            />
          </Space>
        ),
        value: feature.value ?? '--',
        periodEnd: '--',
        periodStart: moment(feature.updatedAt).format('YYYY-MM-DD HH:mm:ss'),
        extras: feature.extras ? (
          <div className="break-all">{JSON.stringify(JSON.parse(feature.extras), null, 10)}</div>
        ) : (
          '--'
        ),
      });
    }

    return tableData;
  };

  return (
    <Modal
      destroyOnClose
      footer={''}
      title={
        <Space className="pb-2">
          <HistoryOutlined />
          Feature History
        </Space>
      }
      open={isOpen}
      onCancel={() => setIsOpen(false)}
      width={1100}
    >
      <Space direction="vertical" className="w-full p-3 mb-4 bg-gray-100 rounded-md">
        <span>
          <b>Name:</b> {feature?.name}
        </span>
        {feature?.type ? TypeTagHelper.TagType(feature!.type, feature?.subType) : null}
      </Space>
      <Table
        columns={tableHeader}
        dataSource={getTableData()}
        rowClassName={(row) => (row.current ? 'border-black border-t-5' : '')}
        scroll={{ x: 1000 }}
      />
    </Modal>
  );
};

export default HistoryModal;
