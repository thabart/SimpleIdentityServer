import React, { Component } from "react";
import { Route, Redirect } from 'react-router-dom';
import { SessionService } from './services';

import Layout from './layout';
import { Login, About, Connections, Logs, Settings, Cache, Manage, Tools, Resources } from './components';

export const routes = <Layout>
    <Route exact path='/' component={About} />
    <Route exact path='/login' component={Login} />
    <Route exact path='/about' component={About} />
    <Route exact path='/connections' component={Connections} />
    <Route exact path='/logs/:action?/:subaction?' component={Logs} />
    <Route exact path='/settings/:action?' component={Settings} />
    <Route exact path='/cache' component={Cache} />
    <Route exact path='/manage' component={Manage} />
    <Route exact path='/tools/:action?' component={Tools} />
    <Route exact path='/resources' component={Resources} />
</Layout>;