import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ScopeService } from '../../services';
import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter, TablePagination } from 'material-ui/Table';
import { Popover, IconButton, Menu, MenuItem, Checkbox } from 'material-ui';

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

        this.refreshData = this.refreshData.bind(this);

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

    refreshData() {
        var self = this;
        self.setState({
            isLoading: true
        });
        ScopeService.search({ start_index: 0, count: 500 }, self.props.type).then(function (result) {
            var data = [];
            if (result.content) {
                result.content.forEach(function (client) {
                    data.push({
                        name: client['name'],
                        type: client['type']
                    });
                });
            }
            
            self.setState({
                isLoading: false,
                data: data,
                pages: 100
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
        var rows = [];
        if (self.state.data) {
            self.state.data.forEach(function(record) {
                rows.push((
                    <TableRow hover role="checkbox" key={record.name}>
                        <TableCell><Checkbox /></TableCell>
                        <TableCell>{record.name}</TableCell>
                        <TableCell>{record.type}</TableCell>
                    </TableRow>
                ));
            });
        }
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
                            </div>
                            <div className="body">
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell></TableCell>
                                            <TableCell>{t('name')}</TableCell>
                                            <TableCell>{t('status')}</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {rows}
                                    </TableBody>
                                    <TableFooter>
                                    </TableFooter>
                                </Table>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        this.refreshData();
    }
}

export default translate('common', { wait: process && !process.release })(ScopeComponent);