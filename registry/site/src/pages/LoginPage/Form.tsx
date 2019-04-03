import Button from 'antd/lib/button'
import Form, { FormComponentProps } from 'antd/lib/form'
import Icon from 'antd/lib/icon'
import Input from 'antd/lib/input'
import React from 'react'


import styles from './Form.module.scss'


type FormProps = FormComponentProps & {
  onSubmit(name: string, password: string): void;
}

class LoginFormInternal extends React.PureComponent<FormProps> {
  public handleSubmit = (e: React.FormEvent<unknown>) => {
    e.preventDefault()
    this.props.form.validateFields((err, values: { login: string, password: string }) => {
      if (!err)
        this.props.onSubmit(values.login, values.password)
    })
  }

  public render() {
    const { getFieldDecorator } = this.props.form

    return (
      <Form onSubmit={this.handleSubmit} className={styles.form}>
        <Form.Item>
          {getFieldDecorator('login', {
            rules: [{ message: 'Please input your Login!', required: true }],
          })(
            <Input
              prefix={<Icon type="user" />}
              placeholder="name"
              autoComplete="username"
            />,
          )}
        </Form.Item>
        <Form.Item>
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
        <Button htmlType="submit" className="submit">
          Log in
        </Button>
      </Form>
    )
  }
}

export const LoginForm = Form.create()(LoginFormInternal)
