import Button from 'antd/lib/button'
import Form, { FormComponentProps } from 'antd/lib/form'
import Icon from 'antd/lib/icon'
import Input from 'antd/lib/input'
import * as React from 'react'


import styles from './Form.module.scss'


interface FormProps extends FormComponentProps {
  onSubmit: (name: string, password: string) => void
}

class LoginForm extends React.PureComponent<FormProps>{
  handleSubmit = (e: React.FormEvent<any>) => {
    e.preventDefault()
    this.props.form.validateFields((err, values) => {
      if (!err)
        this.props.onSubmit(values.login, values.password)
    })
  }

  render() {
    const { getFieldDecorator } = this.props.form

    return (
      <Form onSubmit={this.handleSubmit} className={styles.form}>
        <Form.Item>
          {getFieldDecorator('login', {
            rules: [{ required: true, message: 'Please input your Login!' }],
          })(
            <Input prefix={<Icon type="user" />}
              placeholder="name"
              autoComplete="username" />
          )}
        </Form.Item>
        <Form.Item>
          {getFieldDecorator('password', {
            rules: [{ required: true, message: 'Please input your Password!' }],
          })(
            <Input prefix={<Icon type="lock" />}
              type="password"
              placeholder="password"
              autoComplete="current-password" />
          )}
        </Form.Item>
        <Button type="primary" htmlType="submit" className="submit">
          Log in
        </Button>
      </Form>
    )
  }
}

export default Form.create()(LoginForm)