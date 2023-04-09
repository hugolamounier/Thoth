import React, { useState } from 'react';
import { useForm } from 'antd/lib/form/Form';
import { FeatureFlag, FeatureFlagsTypes } from '../../../models/featureFlag';
import { Form, Input, Modal, Select, Space, Switch } from 'antd';
import { FileAddOutlined } from '@ant-design/icons';
import TextArea from 'antd/lib/input/TextArea';

interface CreateModalInterface {
  isOpen: boolean;
  setIsOpen: (state: boolean) => void;
  onSubmitForm: (values: any) => void;
  isLoading: boolean;
}

const CreateModal = ({ isOpen, setIsOpen, onSubmitForm, isLoading }: CreateModalInterface) => {
  const [addFeatureFlagForm] = useForm<FeatureFlag>();
  const [typeSelect, setTypeSelect] = useState<FeatureFlagsTypes | undefined>(undefined);

  return (
    <Modal
      destroyOnClose
      afterClose={() => {
        addFeatureFlagForm.resetFields();
        setTypeSelect(undefined);
      }}
      title={
        <Space>
          <FileAddOutlined /> <span> Create new feature flag</span>
        </Space>
      }
      open={isOpen}
      okButtonProps={{ loading: isLoading }}
      onOk={() => addFeatureFlagForm.submit()}
      onCancel={() => setIsOpen(false)}
      okText="Create"
      width={700}
    >
      <Form form={addFeatureFlagForm} className="py-4" layout="vertical" onFinish={onSubmitForm}>
        <Form.Item name="name" label="Name" rules={[{ required: true }]}>
          <Input />
        </Form.Item>
        <Form.Item name="type" label="Type" rules={[{ required: true }]}>
          <Select
            showSearch
            placeholder="Select flag type"
            optionFilterProp="children"
            onChange={(value, option) => setTypeSelect(value)}
            filterOption={(input, option) =>
              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
            options={Object.keys(FeatureFlagsTypes)
              .filter((v) => isNaN(Number(v)))
              .map((key, index) => {
                return {
                  value: FeatureFlagsTypes[key as keyof typeof FeatureFlagsTypes],
                  label: key,
                };
              })}
          />
        </Form.Item>
        {typeSelect !== undefined && typeSelect !== FeatureFlagsTypes.Boolean ? (
          <Form.Item name="filterValue" label="Filter value" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
        ) : null}
        <Form.Item name="value" label="Initial State" valuePropName="checked">
          <Switch defaultChecked={false} unCheckedChildren="Off" checkedChildren="On" />
        </Form.Item>
        <Form.Item name="description" label="Description">
          <TextArea />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default CreateModal;
