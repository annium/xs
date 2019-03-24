import Col from 'antd/lib/col'
import Icon from 'antd/lib/icon'
import Input from 'antd/lib/input'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import React from 'react'

import { inject, Store } from '../../store'
import { getCenteredLayout } from '../../utils/layout'

import styles from './index.module.scss'
import { UpdateUserForm } from './UpdateUserForm'


type Props = Pick<Store, 'auth'>

export const SettingsPage = inject<{}, Pick<Store, 'auth'>>(
  ({ auth }) => ({ auth }),
  ({ auth }: Props) => (
    <div className={styles.page}>
      <Row>
        <Col {...getCenteredLayout(24, 16, 12, 10, 8)}>
          <h1>Settings</h1>
          <h2>Credentials</h2>
          <UpdateUserForm name={auth.user.data!.name} onSubmit={handleUpdate(auth)} />
          <h2>API Token</h2>
          <Input
            disabled={true}
            value={auth.user.data!.apiToken}
            suffix={<Icon type="sync" onClick={handleUpdateToken(auth)} />}
          />
        </Col>
      </Row>
    </div >
  ),
)

const handleUpdate = (auth: Props['auth']) => (name: string, password: string) => auth
  .update(name, password)
  .then(() => message.success('credentials updated'))
  .catch(error => message.error(`credentials save failed: ${error}`))

const handleUpdateToken = (user: Props['auth']) => () => user
  .updateToken()
  .then(() => message.success('token updated'))
  .catch(error => message.error(`token update failed: ${error}`))
