import React, { Component } from "react";
import { Route, Redirect } from 'react-router-dom';
import { SessionService } from './services';

import Layout from './layout';
import { Login, About, Logs, Resources, ViewAggregate, ViewLog,
 OAuthClients, OpenidClients, OAuthScopes, OpenidScopes, ResourceOwners, ViewResource, ViewClient, ViewScope,
 AddScope } from './components';

export const routes = (<Layout>
    <Route exact path='/' component={About} />
    <Route exact path='/about' component={About} />
    <Route exact path='/login' component={Login} />
    { !process.env.IS_LOG_DISABLED && (<Route exact path='/logs/:action?/:subaction?' component={Logs} />) }
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/authclients' component={OAuthClients} />) }
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/openidclients' component={OpenidClients} />) }
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/authscopes' component={OAuthScopes} />) }
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/openidscopes' component={OpenidScopes} />) }
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/resourceowners' component={ResourceOwners} />) }
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/viewClient/:type/:id' component={ViewClient} />)}
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/viewScope/:type/:id' component={ViewScope} />)}
    { !process.env.IS_MANAGE_DISABLED && (<Route exact path='/addScope/:type' component={AddScope} />)}
    { !process.env.IS_RESOURCES_DISABLED && (<Route exact path='/resources/:action?' component={Resources} />)}
    { !process.env.IS_RESOURCES_DISABLED && (<Route exact path='/resource/:id' component={ViewResource} />)}
    { !process.env.IS_LOG_DISABLED && (<Route exact path="/viewaggregate/:id" component={ViewAggregate} /> )}
    { !process.env.IS_LOG_DISABLED && (<Route exact path="/viewlog/:id" component={ViewLog} /> )}
</Layout>);