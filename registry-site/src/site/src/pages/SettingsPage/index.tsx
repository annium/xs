import Col, { ColSize } from 'antd/lib/col'
import Icon from 'antd/lib/icon'
import Input from 'antd/lib/input'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Store } from '../../store'

import styles from './index.module.scss'
import UpdateUserForm from './UpdateUserForm'


type Props = Pick<Store, 'user'> & RouteComponentProps

const log = console.log.bind(console, 'SettingsPage')
@inject((stores: Store) => ({ user: stores.user }))
@observer
export default class SettingsPage extends React.Component<Props> {
  render() {
    log('render')
    const { user } = this.props

    const handleUpdate = (name: string, password: string) => user
      .update(name, password)
      .then(() => message.success('credentials updated'),
        error => message.error('credentials save failed: ' + error))

    const handleUpdateToken = () => user
      .updateToken()
      .then(() => message.success('token updated'),
        error => message.error('token update failed: ' + error))

    const layout = this.layout()

    return (
      <div className={styles.page}>
        <Row>
          <Col {...layout}>
            <h1>Settings</h1>
            <h2>Credentials</h2>
            <UpdateUserForm name={user.data!.name} onSubmit={handleUpdate} />
            <h2>API Token</h2>
            <Input disabled value={user.data!.apiToken} suffix={<Icon type="sync" onClick={handleUpdateToken} />} />
          </Col>
        </Row>
      </div >
    )
  }
  private layout(): { [key: string]: ColSize } {
    return {
      xs: { span: 24 },
      sm: { offset: 4, span: 16 },
      md: { offset: 6, span: 12 },
      lg: { offset: 7, span: 10 },
      xl: { offset: 8, span: 8 },
    }
  }
}