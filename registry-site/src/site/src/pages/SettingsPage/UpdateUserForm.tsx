import Button from 'antd/lib/button'
import Form, { FormComponentProps } from 'antd/lib/form'
import Icon from 'antd/lib/icon'
import Input from 'antd/lib/input'
import * as React from 'react'


import styles from './UpdateUserForm.module.scss'


interface FormProps extends FormComponentProps {
  name: string
  onSubmit: (name: string, password: string) => void
}

class UpdateUserForm extends React.PureComponent<FormProps>{
  handleSubmit = (e: React.FormEvent<any>) => {
    e.preventDefault()
    this.props.form.validateFields((err, values) => {
      if (!err)
        this.props.onSubmit(values.login, values.password)
    })
  }

  render() {
    const { name, form: { getFieldDecorator } } = this.props

    const inputLayout = { labelCol: { span: 6 }, wrapperCol: { span: 18 } }
    const buttonLayout = { wrapperCol: { offset: 6, span: 18 } }

    return (
      <Form onSubmit={this.handleSubmit} className={styles.form} layout="horizontal">
        <Form.Item label="Name" {...inputLayout}>
          {getFieldDecorator('login', {
            initialValue: name,
            rules: [{ required: true, message: 'Please input your Login!' }],
          })(
            <Input prefix={<Icon type="user" />}
              placeholder="name"
              autoComplete="username" />
          )}
        </Form.Item>
        <Form.Item label="Password" {...inputLayout}>
          {getFieldDecorator('password', {
            rules: [{ required: true, message: 'Please input your Password!' }],
          })(
            <Input prefix={<Icon type="lock" />}
              type="password"
              placeholder="password"
              autoComplete="current-password" />
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
}

export default Form.create()(UpdateUserForm)