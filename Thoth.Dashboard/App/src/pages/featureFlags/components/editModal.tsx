import React from 'react';
import { useForm } from 'antd/lib/form/Form';
import { FeatureFlagsTypes, FeatureManager, FeatureTypes } from '../../../models/featureManager';
import { Form, Input, Modal, Space, Switch } from 'antd';
import { FileTextOutlined } from '@ant-design/icons';
import TextArea from 'antd/lib/input/TextArea';

interface EditModalInterface {
  isOpen: boolean;
  setIsOpen: (state: boolean) => void;
  onSubmitForm: (values: any) => void;
  isLoading: boolean;
  feature?: FeatureManager;
}

const EditModal = ({ isOpen, setIsOpen, onSubmitForm, isLoading, feature }: EditModalInterface) => {
  const [editFeatureForm] = useForm<FeatureManager>();

  return (
    <Modal
      destroyOnClose
      afterClose={() => {
        editFeatureForm.resetFields();
      }}
      title={
        <Space>
          <FileTextOutlined /> <span>Edit Feature</span>
        </Space>
      }
      open={isOpen}
      okButtonProps={{ loading: isLoading }}
      onOk={() => editFeatureForm.submit()}
      onCancel={() => setIsOpen(false)}
      okText="Edit"
      width={700}
    >
      <Form
        form={editFeatureForm}
        className="py-4"
        layout="vertical"
        onFinish={(data: FeatureManager) => {
          data.subType = feature?.subType;
          data.type = feature!.type;
          onSubmitForm(data);
        }}
      >
        <Form.Item label="Name" initialValue={feature?.name}>
          <Input value={feature?.name} defaultValue={feature?.name} disabled />
        </Form.Item>
        {feature?.type === FeatureTypes.EnvironmentVariable ||
        feature?.subType === FeatureFlagsTypes.PercentageFilter ? (
          <Form.Item
            name="value"
            label="Value"
            initialValue={feature.value}
            rules={[{ required: true }]}
          >
            <Input defaultValue={feature.value} />
          </Form.Item>
        ) : null}
        <Form.Item
          name="enabled"
          label="Initial State"
          initialValue={feature?.enabled}
          valuePropName="checked"
          hidden={feature?.type === FeatureTypes.EnvironmentVariable}
        >
          <Switch defaultChecked={feature?.enabled} unCheckedChildren="Off" checkedChildren="On" />
        </Form.Item>
        <Form.Item name="description" label="Description" initialValue={feature?.description}>
          <TextArea defaultValue={feature?.description} />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default EditModal;
