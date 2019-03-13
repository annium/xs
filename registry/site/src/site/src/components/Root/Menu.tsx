import Icon from 'antd/lib/icon'
import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { NavLink, RouteComponentProps, withRouter } from 'react-router-dom'

import { Store } from '../../store'


import styles from './Menu.module.scss'


type Props = Partial<Pick<Store, 'user'>> & RouteComponentProps

class MenuInternal extends React.Component<Props> {
  public render() {
    const { data, logout } = this.props.user!

    return (
      <div className={styles.menu}>
        <NavLink className={styles.item} exact={true} activeClassName={styles.isActiveItem} to="/">
          <Icon type="home" /> Home
        </NavLink>
        <NavLink className={styles.item} exact={true} activeClassName={styles.isActiveItem} to="/packages">
          <Icon type="search" /> Packages
        </NavLink>
        <div className={styles.separator} />
        <div className={styles.info}>Hi, {data!.name}</div>
        <div className={styles.separator} />
        <NavLink className={styles.item} exact={true} activeClassName={styles.isActiveItem} to="/settings">
          <Icon type="setting" /> Settings
        </NavLink>
        <NavLink className={styles.item} to="/login" onClick={logout}>
          <Icon type="logout" /> Log out
        </NavLink>
      </div>
    )
  }
}

export const Menu = withRouter(inject((stores: Store) => ({ user: stores.user }))(observer(MenuInternal)))
