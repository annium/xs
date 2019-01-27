import Icon from 'antd/lib/icon'
import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { NavLink } from 'react-router-dom'

import { UserStore } from '../../data/user'
import { Store } from '../../store'


import styles from './Menu.module.scss'


export interface Props {
  user?: UserStore
}

@inject((stores: Store) => ({ user: stores.user }))
@observer
export default class Menu extends React.Component<Props> {
  render() {
    const { data, logout } = this.props.user!

    return (
      <div className={styles.menu}>
        <NavLink className={styles.item} exact activeClassName={styles.isActiveItem} to="/">
          <Icon type="home" /> Home
        </NavLink>
        <div className={styles.separator} />
        <div className={styles.info}>Hi, {data!.name}</div>
        <div className={styles.separator} />
        <div className={styles.item} onClick={logout}>
          <Icon type="logout" /> Log out
        </div>
      </div>
    )
  }
}