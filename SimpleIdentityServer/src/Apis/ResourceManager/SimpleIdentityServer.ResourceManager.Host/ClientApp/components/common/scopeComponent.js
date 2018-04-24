import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ScopeService } from '../../services';

import { Popover, IconButton, Menu, MenuItem } from 'material-ui';
import ReactTable from 'react-table'


const columns = [
    { Header: 'Name', accessor: 'name' },
    { Header: 'Type', accessor: 'type', Cell: row => (
            <span>{row.value === 0 ? "Protected API" : "Resource Owner"}</span>
    )}
];

class ScopeComponent extends Component {
    constructor(props, type) {
        super(props);
        this._type = type;
        this.displayPopOver = this.displayPopOver.bind(this);
        this.toggleProperty = this.toggleProperty.bind(this);
        this.fetchData = this.fetchData.bind(this);
        this.state = {
            data: [],
            isLoading: false,
            pages: 0,
            isSettingsOpened: false,
            anchorEl: null,
        };
    }

    displayPopOver(e) {
        this.setState({
            isSettingsOpened: true,
            anchorEl: e.currentTarget
        })
    }

    toggleProperty(name) {
        this.setState({
            [name]: !this.state[name]
        })
    }

    fetchData(state, instance) {
        var self = this;
        self.setState({
            isLoading: true
        });
        var startIndex = state.page * state.pageSize;
        ScopeService.search({ start_index: startIndex, count: state.pageSize }, self.props.type).then(function (result) {
            var data = [];
            if (result.content) {
                result.content.forEach(function (client) {
                    data.push({
                        name: client['name'],
                        type: client['type']
                    });
                });
            }
            
            var pages = Math.round((result.count + state.pageSize - 1) / state.pageSize);
            self.setState({
                isLoading: false,
                data: data,
                pages: pages
            });
        }).catch(function (e) {
            self.setState({
                isLoading: false,
                data: [],
                pages: 0
            });
        });
    }

    render() {
        var self = this;
        const { t } = this.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('scopes')}</h4>
                <i>{t('scopesDescription')}</i>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="header">
                                <h4 style={{display: "inline-block"}}>{t('listOfScopes')}</h4>
                                <div style={{float: "right"}}>
                                    <IconButton iconClassName="material-icons" onClick={self.displayPopOver}>&#xE5D4;</IconButton>                                    
                                    <Popover
                                      open={this.state.isSettingsOpened}
                                      anchorEl={this.state.anchorEl}
                                      anchorOrigin={{horizontal: 'left', vertical: 'bottom'}}
                                      targetOrigin={{horizontal: 'left', vertical: 'top'}}
                                    onRequestClose={() => self.toggleProperty('isSettingsOpened')}
                                    >   
                                        <Menu>
                                            <MenuItem primaryText={t('addScope')} />
                                        </Menu>
                                    </Popover>
                                </div>
                            </div>
                            <div className="body">
                                <ReactTable
                                    data={this.state.data}
                                    loading={this.state.isLoading}
                                    onFetchData={this.fetchData}
                                    pages={this.state.pages}
                                    columns={columns}
                                    defaultPageSize={10}
                                    manual={true}
                                    filterable={false}
                                    showPaginationTop={true}
                                    showPaginationBottom={false}
                                    sortable={false}
                                    className="-striped -highlight"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(ScopeComponent);