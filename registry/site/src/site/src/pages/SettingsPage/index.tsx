import Col from 'antd/lib/col'
import Icon from 'antd/lib/icon'
import Input from 'antd/lib/input'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Store } from '../../store'
import { getCenteredLayout } from '../../utils/layout'

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

    return (
      <div className={styles.page}>
        <Row>
          <Col {...getCenteredLayout(24, 16, 12, 10, 8)}>
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
}