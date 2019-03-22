import Icon from 'antd/lib/icon'
import React from 'react'
import { NavLink, RouteComponentProps, withRouter } from 'react-router-dom'

import { inject, Store } from '../../store'


import styles from './Menu.module.scss'


type Props = Partial<Pick<Store, 'auth'>> & RouteComponentProps

export const Menu = withRouter(inject(
  ({ auth: user }) => ({ user }),
  function Menu({ auth: user }: Props) {
    const { data, logout } = user!

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
  },
))
