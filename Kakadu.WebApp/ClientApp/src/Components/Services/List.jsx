import React from 'react';
import { connect } from 'react-redux';
import { Container, Table, Navbar, NavItem, NavLink } from 'reactstrap';
import { serviceActions } from '../../_actions';
import { Link } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'

class List extends React.Component {
    constructor(props) {
        super(props);

        this.startCapturing = this.startCapturing.bind(this);
        this.stopCapturing = this.stopCapturing.bind(this);
        this.deleteService = this.deleteService.bind(this);
    }
    componentDidMount() {
        this.props.dispatch(serviceActions.getAll());
    }

    startCapturing(serviceCode) {
        console.log('recording', serviceCode);
    }

    stopCapturing(serviceCode) {
        console.log('stopping', serviceCode);
    }

    deleteService(serviceCode) {
        this.props.dispatch(serviceActions.remove(serviceCode));
    }

    render() {
        const { services } = this.props;
        return (
            <Container>
                {services.error && <span className="text-danger">ERROR: {services.error}</span>}
                <Navbar className="navbar-expand-sm navbar-toggleable-sm pl-0 mb-0" light>
                    <ul className="navbar-nav mr-auto">
                    <NavItem>
                        <NavLink href="/services/add" className="btn btn-success text-white" to="/services/add">Add service</NavLink>
                    </NavItem>
                    </ul>
                </Navbar>
                <Table bordered hover responsive>
                    <thead>
                        <tr>
                            <th>Code</th>
                            <th>Name</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {services.loading && <tr><td colSpan="3">Loading services...</td></tr>}    
                        {services.items && services.items.map((service, index) =>
                            <tr key={service.id}>
                                <td>
                                    <Link to={`/services/edit/${service.code}`}>{service.code}</Link>
                                </td>
                                <td>{service.name}</td>
                                <td>
                                    {service.isRecording ?
                                        <FontAwesomeIcon
                                            cursor="pointer"
                                            title="stop capturing"
                                            icon="stop"
                                            className="icon-margin-right-10"
                                            onClick={() => this.stopCapturing(service.code)}
                                        /> :
                                        <FontAwesomeIcon
                                            cursor="pointer"
                                            title="start capturing"
                                            icon="circle"
                                            color="red"
                                            className="icon-margin-right-10"
                                            onClick={() => this.startCapturing(service.code)}
                                        />
                                    }
                                    <Link to={`/services/edit/${service.code}`}>
                                        <FontAwesomeIcon cursor="pointer" title="edit" icon="edit" className="icon-margin-right-10" />
                                    </Link>
                                    <FontAwesomeIcon 
                                        cursor="pointer"
                                        title="delete"
                                        icon="trash-alt"
                                        className="icon-margin-right-10"
                                        onClick={() => this.deleteService(service.code)}
                                    />
                                </td>
                            </tr>
                        )}
                    </tbody>
                </Table>
            </Container>
        );
    }
}

function mapStateToProps(state) {
    const { services, authentication } = state;
    const { user } = authentication;
    return {
        user,
        services
    };
}

const connectedServicesPage = connect(mapStateToProps)(List);
export { connectedServicesPage as List };