import React, { Component } from "react";
import { Route, Redirect } from 'react-router-dom';
import { SessionService } from './services';

import Layout from './layout';
import { Login, About, Connections, Logs, Settings, Cache, Manage, Tools, Resources } from './components';

export const routes = (<Layout>
    <Route exact path='/' component={About} />
    <Route exact path='/about' component={About} />
    <Route exact path='/login' component={Login} />
    { !process.env.IS_CONNECTIONS_DISABLED && (<Route exact path='/connections' component={Connections} />) }
    { !process.env.IS_LOG_DISABLED && (<Route exact path='/logs/:action?/:subaction?' component={Logs} />) }
    { !process.env.IS_SETTINGS_DISABLED && (<Route exact path='/settings/:action?' component={Settings} /> ) }
    { !process.env.IS_CACHE_DISABLED && (<Route exact path='/cache' component={Cache} /> )}
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/manage' component={Manage} />) }
    { !process.env.IS_TOOLS_DISABLED && (<Route exact path='/tools/:action?' component={Tools} />) }
    { !process.env.IS_RESOURCES_DISABLED && (<Route exact path='/resources' component={Resources} />) }
</Layout>);