import Button from 'antd/lib/button'
import Form, { FormComponentProps } from 'antd/lib/form'
import Icon from 'antd/lib/icon'
import Input from 'antd/lib/input'
import React from 'react'


import styles from './UpdateUserForm.module.scss'


type FormProps = FormComponentProps & {
  name: string
  onSubmit(name: string, password: string): void;
}

class UpdateUserFormInternal extends React.PureComponent<FormProps> {
  public render() {
    const { name, form: { getFieldDecorator } } = this.props

    const inputLayout = { labelCol: { span: 6 }, wrapperCol: { span: 18 } }
    const buttonLayout = { wrapperCol: { offset: 6, span: 18 } }

    return (
      <Form onSubmit={this.handleSubmit} className={styles.form} layout="horizontal">
        <Form.Item label="Name" {...inputLayout}>
          {getFieldDecorator('login', {
            initialValue: name,
            rules: [{ message: 'Please input your Login!', required: true }],
          })(
            <Input
              prefix={<Icon type="user" />}
              placeholder="name"
              autoComplete="username"
            />,
          )}
        </Form.Item>
        <Form.Item label="Password" {...inputLayout}>
          {getFieldDecorator('password', {
            rules: [{ message: 'Please input your Password!', required: true }],
          })(
            <Input
              prefix={<Icon type="lock" />}
              type="password"
              placeholder="password"
              autoComplete="current-password"
            />,
          )}
        </Form.Item>
        <Form.Item {...buttonLayout}>
          <Button type="primary" htmlType="submit" className="submit">
            Update credentials
          </Button>
        </Form.Item>
      </Form>
    )
  }

  private readonly handleSubmit = (e: React.FormEvent<unknown>) => {
    e.preventDefault()
    this.props.form.validateFields((err, values: { login: string, password: string }) => {
      if (!err)
        this.props.onSubmit(values.login, values.password)
    })
  }
}

export const UpdateUserForm = Form.create()(UpdateUserFormInternal)
