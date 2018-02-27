import React, { Component } from "react";
import { Route, Redirect } from 'react-router-dom';
import { SessionService } from './services';

import Layout from './layout';
import { Login, About, Connections } from './components';

export const routes = <Layout>
    <Route exact path='/' component={About} />
    <Route exact path='/login' component={Login} />
    <Route exact path='/about' component={About} />
    <Route exact path='/connections' component={Connections} />
</Layout>;