import _thread as thread
import json
import time
import re
import pygame
from pygame import RESIZABLE, DOUBLEBUF, HWSURFACE
from websocket import create_connection, WebSocketConnectionClosedException

import lock

GRAY = (100, 100, 100)
NAVYBLUE = (60, 60, 100)
WHITE = (255, 255, 255)
RED = (255, 0, 0)
GREEN = (0, 255, 0)
BLUE = (0, 0, 255)
YELLOW = (255, 255, 0)
ORANGE = (255, 128, 0)
PURPLE = (255, 0, 255)
CYAN = (0, 255, 255)
BLACK = (0, 0, 0)

GREEN_LIGHT = [149, 217, 104, 10]
YELLOW_LIGHT = [205, 217, 104, 10]
PURPLE_LIGHT = [162, 134, 222, 10]
BLUE_LIGHT = [174, 218, 233, 10]

RASTER_COLORS = [GREEN_LIGHT, YELLOW_LIGHT, PURPLE_LIGHT, BLUE_LIGHT]
VECTOR_COLORS = [RED, WHITE, BLUE, ORANGE, YELLOW]
#COLORS = [NAVYBLUE, RED, WHITE, BLUE, ORANGE, PURPLE, CYAN]  # without WHITE for background

COLORS = RASTER_COLORS

WINDOW_SIZE = 800, 800

zombie_image = pygame.transform.flip(pygame.transform.scale(pygame.image.load("Skull and Pizza illustration.jpg"), (16, 16)), False, True)
zombie_rect = zombie_image.get_rect()
human_image = pygame.transform.flip(pygame.transform.scale(pygame.image.load("vecteezy_post-apocalyptic-character-poor-people-in-damaged-city-war_3498661.jpg"), (16, 16)), False, True)

class Visualization:
    def __init__(self):
        pygame.init()
        pygame.display.set_caption("MARS-Mini-VIS")

        self.programIcon = pygame.image.load('icon.png')
        pygame.display.set_icon(self.programIcon)

        self.clock = pygame.time.Clock()
        self.WINDOW_SIZE = [800, 800]
        self.WORLD_SIZE = 0, 0, 100, 100  # used for scaling
        self.BORDER_WIDTH_PIXEL = -20
        self.font = pygame.font.Font('freesansbold.ttf', 12)
        self.text = self.font.render('Tick: 0', True, YELLOW)
        self.fps_text = self.font.render('FPS: 0', True, YELLOW)
        self.desired_fps = self.font.render('Desired FPS: 0', True, YELLOW)
        self.textRect = self.text.get_rect()
        self.fpsTextRect = self.fps_text.get_rect()
        self.desired_fpsRect = self.desired_fps.get_rect()

        self.l = lock.RWLock()
        self.pressed_up = False
        self.pressed_down = False
        self.entities = {}
        self.point_features = []
        self.line_features = []
        self.ring_features = []
        self.polygon_features = []
        self.raster_metadata = {}
        self.tick_display = [False, 0, 1000]
        self.fps = 60
        self.run = True
        self.uri = "ws://127.0.0.1:4567/vis"
        self.ws = None
        self.desired_fps = self.fps
        self.time_to_wait_milliseconds = 10
        self.borderColor = (255, 255, 255)
        self.barColor = (0, 128, 0)
        self.set_window_relations(800, 800)

    def set_window_relations(self, width, height):
        self.WINDOW_SIZE[0] = width
        self.WINDOW_SIZE[1] = height
        self.textRect.center = (30, height - 12)
        self.fpsTextRect.center = (120, height - 12)
        self.desired_fpsRect.center = (230, height - 12)
        self.barPos = (10, height - 40)
        self.barSize = (self.WINDOW_SIZE[0] - 20, 20)
        self.screen = pygame.display.set_mode((self.WINDOW_SIZE[0], height),
                                              HWSURFACE | DOUBLEBUF | RESIZABLE)

    def get_socket(self):
        ws = None
        while ws is None:
            try:
                ws = create_connection(self.uri)
                print("Connecting to simulation ...")
            except (ConnectionResetError, ConnectionRefusedError, TimeoutError, WebSocketConnectionClosedException):
                print("Waiting for running simulation ... ")
                time.sleep(2)
                ws = None
        return ws

    def load_data(self):
        if self.ws is None:
            self.ws = self.get_socket()
        try:
            message = re.sub('(?<=\d),(?=\d)', '.',self.ws.recv())
            # print(message)
            data = json.loads(message)

            self.l.acquire_write()
            if "currentTick" in data:
                self.tick_display[1] = data["currentTick"]
            if "maxTicks" in data:
                self.tick_display[2] = data["maxTicks"]

            if "entities" in data:
                entities_points = data["entities"]
                self.entities[data['t']] = entities_points
            if "worldSize" in data:
                world_data = data["worldSize"]
                if world_data["maxX"] > 0:
                    self.WORLD_SIZE = world_data["minX"], world_data["minY"], world_data["maxX"], world_data["maxY"]
            if "vectors" in data:
                layers_data = data["vectors"]
                for feature in layers_data:
                    geometry = feature["geometry"]
                    geometry_type = geometry["type"]
                    if geometry_type == "LineString":
                        self.line_features.append(geometry)
                    elif geometry_type == "LineRing":
                        self.ring_features.append(geometry)
                    elif geometry_type == "Point":
                        self.point_features.append(geometry)
                    elif geometry_type == "Polygon":
                        self.polygon_features.append(geometry)
            if "rasters" in data:
                raster_data = data["rasters"]
                for raster in raster_data:
                    proxy_key = raster["t"]
                    self.raster_metadata[proxy_key] = raster

            self.tick_display[0] = True
            self.l.release_write()
        except (ConnectionResetError, ConnectionRefusedError, WebSocketConnectionClosedException):
            self.ws.shutdown()
            self.entities.clear()
            if self.l.get_active_writer() > 0:
                self.l.release_write()
            self.screen.fill(BLACK)
            self.ws = None

    def visualize_content(self):

        self.clock.tick(self.desired_fps)
        self.load_data()

        if not self.tick_display[0]:
            return

        self.screen.fill(BLACK)

        #window_area = (10, 10,self.WINDOW_SIZE[0] - 10, self.WINDOW_SIZE[1] - 10)

        delta_x = self.WORLD_SIZE[2] - self.WORLD_SIZE[0]
        delta_y = self.WORLD_SIZE[3] - self.WORLD_SIZE[1]

        scale_x = self.WINDOW_SIZE[0] / delta_x
        scale_y = self.WINDOW_SIZE[1] / delta_y

        #scale_x = window_area[2] / delta_x
        #scale_y = window_area[3] / delta_y

        #scale_x = (self.WINDOW_SIZE[0] - (self.WORLD_SIZE[0])) / self.WORLD_SIZE[2]
        #scale_y = (self.WINDOW_SIZE[1] - (self.WORLD_SIZE[1] + 50)) / self.WORLD_SIZE[3]

        #line_width = int((scale_x + scale_y) / 2)
        line_width = 1

        surface = pygame.Surface(self.WINDOW_SIZE)

        for raster_key in self.raster_metadata.keys():
            raster = self.raster_metadata[raster_key]
            width = raster["cellWidth"] * scale_x
            height = raster["cellHeight"] * scale_y
            cells_with_value = raster["cells"]
            for cell in cells_with_value:
                x = ((cell[0] - self.WORLD_SIZE[0]) * scale_x) + self.BORDER_WIDTH_PIXEL
                y = ((cell[1] - self.WORLD_SIZE[1]) * scale_y)
                value = cell[2] % 255
                color = RASTER_COLORS[raster_key % len(RASTER_COLORS)]
                color[3] = value
                pygame.draw.rect(surface, color, pygame.Rect(x - width / 2, y - height / 2, width + 1, height + 1))

        for geometry in self.point_features:
            point = geometry["coordinates"]
            point_feature = (((float(point[0]) - self.WORLD_SIZE[0]) * scale_x) + self.BORDER_WIDTH_PIXEL,
                             ((float(point[1]) - self.WORLD_SIZE[1]) * scale_y) )
            pygame.draw.circle(surface, BLUE, (point_feature[0], point_feature[1]), line_width, 0)

        for geometry in self.line_features:
            line_feature = [(
                (float(x[0] - self.WORLD_SIZE[0]) * scale_x) + self.BORDER_WIDTH_PIXEL,
                ((float(x[1]) - self.WORLD_SIZE[1]) * scale_y)) for x in geometry["coordinates"]]
            pygame.draw.lines(surface, PURPLE, False, line_feature, line_width)

        for geometry in self.polygon_features:
            polygon_geometry_list = geometry["coordinates"]
            for coordinates in polygon_geometry_list:
                pointlist = [(
                    (float(x[0] - self.WORLD_SIZE[0]) * scale_x) + self.BORDER_WIDTH_PIXEL,
                    (float(x[1] - self.WORLD_SIZE[1]) * scale_y) ) for x in coordinates]
                pygame.draw.polygon(surface, GREEN, pointlist, 0)

        for geometry in self.ring_features:
            pointlist = [(
                ((float(x[0]) - self.WORLD_SIZE[0]) * scale_x) + self.BORDER_WIDTH_PIXEL,
                ((float(x[1]) - self.WORLD_SIZE[1]) * scale_y) ) for x in geometry["coordinates"]]
            pygame.draw.polygon(surface, ORANGE, pointlist, line_width)

        for type_key in self.entities.keys():
            for entity in self.entities[type_key]:
                x = entity["x"]
                y = entity["y"]

                #if type_key==1:
                #    surface.blit(human_image, (((x - self.WORLD_SIZE[0]) * scale_x) + self.BORDER_WIDTH_PIXEL,
                #                    ((y - self.WORLD_SIZE[1]) * scale_y) ))
                #else:
                #    surface.blit(zombie_image, (((x - self.WORLD_SIZE[0]) * scale_x) + self.BORDER_WIDTH_PIXEL,
                #                    ((y - self.WORLD_SIZE[1]) * scale_y) ))

                pygame.draw.circle(surface, COLORS[type_key % len(COLORS)],
                                   (((x - self.WORLD_SIZE[0]) * scale_x) + self.BORDER_WIDTH_PIXEL,
                                    ((y - self.WORLD_SIZE[1]) * scale_y) ),
                                   line_width, 0)

        flipped = pygame.transform.flip(surface, False, True)
        self.screen.blit(flipped, (10, -50))

        self.screen.blit(self.font.render(f'Tick: {self.tick_display[1]}', True, WHITE), self.textRect)
        self.screen.blit(self.font.render(f'FPS: {round(self.clock.get_fps(), 2)}', True, WHITE), self.fpsTextRect)
        self.screen.blit(
            self.font.render(f'Desired FPS: {self.desired_fps} (use up- and down arrows to change)', True, WHITE),
            self.desired_fpsRect)

        if self.tick_display[2] != 0:
            progress = self.tick_display[1] / self.tick_display[2]
            self.draw_progress(self.barPos, self.barSize, self.borderColor, self.barColor, progress)

        pygame.display.update()

    def draw_progress(self, pos, size, border_c, bar_c, progress):
        pygame.draw.rect(self.screen, border_c, (*pos, *size), 1)
        inner_pos = (pos[0] + 3, pos[1] + 3)
        inner_size = ((size[0] - 6) * progress, size[1] - 6)
        pygame.draw.rect(self.screen, bar_c, (*inner_pos, *inner_size))

    def handle_inputs(self):


        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                print("Stop visualization ...")
                self.run = False
            if event.type == pygame.VIDEORESIZE:
                width, height = event.size
                if width < 500:
                    width = 500
                if height < 500:
                    height = 500
                self.set_window_relations(width, height)
            if event.type == pygame.KEYDOWN:
                if self.ws is not None:
                    if event.key == pygame.K_LEFT:
                        self.time_to_wait_milliseconds = (self.time_to_wait_milliseconds - 3)
                        if self.time_to_wait_milliseconds < 0:
                            self.time_to_wait_milliseconds = 3
                        self.ws.send(json.dumps({"timeToWaitInMilliseconds": self.time_to_wait_milliseconds}))
                    if event.key == pygame.K_RIGHT:
                        self.time_to_wait_milliseconds = (self.time_to_wait_milliseconds + 10)
                        self.ws.send(json.dumps({"timeToWaitInMilliseconds": self.time_to_wait_milliseconds}))
        keys = pygame.key.get_pressed()
        if keys[pygame.K_DOWN]:
            self.desired_fps = (self.desired_fps - 3)
            if self.desired_fps < 0:
                self.desired_fps = 5
        elif keys[pygame.K_UP]:
            self.desired_fps = (self.desired_fps + 3)
            if self.desired_fps >= 1000:
                self.desired_fps = 1000


    def content_loop(self):
        while self.run:
            self.visualize_content()

    def visualization_loop(self):

        thread.start_new_thread(self.content_loop, ())

        while self.run:
            self.clock.tick(self.desired_fps)
            self.handle_inputs()
            # self.visualize_content()

        pygame.quit()


vis = Visualization()
vis.visualization_loop()
