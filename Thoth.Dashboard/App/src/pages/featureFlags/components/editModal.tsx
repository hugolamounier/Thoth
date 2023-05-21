import React, { useState } from 'react';
import { useForm } from 'antd/lib/form/Form';
import { FeatureManager, FeatureFlagsTypes, FeatureTypes } from '../../../models/featureManager';
import { Form, Input, Modal, Select, Space, Switch } from 'antd';
import { FileAddOutlined } from '@ant-design/icons';
import TextArea from 'antd/lib/input/TextArea';

interface EditModalInterface {
  isOpen: boolean;
  setIsOpen: (state: boolean) => void;
  onSubmitForm: (values: any) => void;
  isLoading: boolean;
}

const EditModal = ({ isOpen, setIsOpen, onSubmitForm, isLoading }: EditModalInterface) => {
  const [addFeatureFlagForm] = useForm<FeatureManager>();
  const [typeSelect, setTypeSelect] = useState<FeatureTypes | undefined>(undefined);
  const [subTypeSelect, setSubTypeSelect] = useState<FeatureFlagsTypes | undefined>(undefined);

  return (
    <Modal
      destroyOnClose
      afterClose={() => {
        addFeatureFlagForm.resetFields();
        setTypeSelect(undefined);
      }}
      title={
        <Space>
          <FileAddOutlined /> <span> Create new feature manager</span>
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
            onChange={(value, option) => {
              setTypeSelect(value);
              addFeatureFlagForm.resetFields(['value']);
              if (value === FeatureTypes.EnvironmentVariable) {
                addFeatureFlagForm.setFieldValue('enabled', true);
              } else {
                addFeatureFlagForm.setFieldValue('enabled', false);
              }
            }}
            filterOption={(input, option) =>
              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
            options={Object.keys(FeatureTypes)
              .filter((v) => isNaN(Number(v)))
              .map((key, index) => {
                return {
                  value: FeatureTypes[key as keyof typeof FeatureTypes],
                  label: key,
                };
              })}
          />
        </Form.Item>
        {typeSelect !== undefined && typeSelect === FeatureTypes.FeatureFlag ? (
          <Form.Item name="subType" label="SubType" rules={[{ required: true }]}>
            <Select
              showSearch
              placeholder="Select flag type"
              optionFilterProp="children"
              onChange={(value, option) => setSubTypeSelect(value)}
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
        ) : null}
        {typeSelect !== undefined &&
        ((subTypeSelect !== undefined && subTypeSelect !== FeatureFlagsTypes.Boolean) ||
          typeSelect === FeatureTypes.EnvironmentVariable) ? (
          <Form.Item name="value" label="Value" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
        ) : null}
        <Form.Item
          name="enabled"
          label="Initial State"
          valuePropName="checked"
          hidden={typeSelect === FeatureTypes.EnvironmentVariable}
        >
          <Switch defaultChecked={false} unCheckedChildren="Off" checkedChildren="On" />
        </Form.Item>
        <Form.Item name="description" label="Description">
          <TextArea />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default EditModal;
